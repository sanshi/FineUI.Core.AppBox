import { existsSync, readdirSync, readFileSync, statSync, writeFileSync } from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const toolDirectory = path.dirname(fileURLToPath(import.meta.url));
const repositoryRoot = path.resolve(toolDirectory, '..');
const checkOnly = process.argv.includes('--check');
const decoder = new TextDecoder('utf-8', { fatal: true });
const controlTypeAliases = new Map([
    ['ContentPanel', 'Panel'],
]);

function listFiles(directory, result = []) {
    for (const name of readdirSync(directory)) {
        if (['.git', 'bin', 'obj', 'packages'].includes(name)) continue;
        const filePath = path.join(directory, name);
        if (statSync(filePath).isDirectory()) {
            listFiles(filePath, result);
        } else if (name.endsWith('.cshtml')) {
            result.push(filePath);
        }
    }
    return result;
}

function readUtf8(filePath) {
    return decoder.decode(readFileSync(filePath));
}

function parsePage(source, filePath) {
    if (source.includes('//NoRazorForms')) return null;
    const modelLine = source.match(/^\s*@model\s+([^\r\n]+)/m)?.[1] ?? '';
    if (modelLine.includes('<')) return null;
    const modelMatch = source.match(/^\s*@model\s+([A-Za-z_][A-Za-z0-9_.]*)/m);
    if (!modelMatch) return null;

    // @model 只决定 Razor 运行时类型；历史页面可能保留了过时值。
    // designer 必须与同页 code-behind 的 partial class 合并，因此优先读取后者。
    const codeBehindPath = `${filePath}.cs`;
    const codeBehind = existsSync(codeBehindPath) ? readUtf8(codeBehindPath) : '';
    const namespaceMatch = codeBehind.match(/\bnamespace\s+([A-Za-z_][A-Za-z0-9_.]*)/);
    const classMatch = codeBehind.match(/\bpartial\s+class\s+([A-Za-z_][A-Za-z0-9_]*)/);
    let namespaceName;
    let className;
    if (namespaceMatch && classMatch) {
        namespaceName = namespaceMatch[1];
        className = classMatch[1];
    } else {
        const fullModel = modelMatch[1];
        const separator = fullModel.lastIndexOf('.');
        if (separator < 1) throw new Error(`${filePath} 的 @model 和代码后置文件都无法确定完整类型。`);
        namespaceName = fullModel.slice(0, separator);
        className = fullModel.slice(separator + 1);
    }
    const controls = [];
    const seenIds = new Map();
    const tagPattern = /<f:([A-Za-z_][A-Za-z0-9_.]*)\b([^>]*)>/g;
    for (const match of source.matchAll(tagPattern)) {
        const idMatch = match[2].match(/\bID\s*=\s*"([A-Za-z_][A-Za-z0-9_]*)"/i);
        if (!idMatch) continue;
        const tagName = match[1];
        const typeName = controlTypeAliases.get(tagName) ?? tagName;
        const id = idMatch[1];
        if (seenIds.has(id)) {
            if (seenIds.get(id) !== typeName) {
                throw new Error(`${filePath} 中 ID ${id} 同时对应多个控件类型。`);
            }
            continue;
        }
        seenIds.set(id, typeName);
        controls.push({ typeName, id });
    }
    return { namespaceName, className, controls };
}

function renderDesigner(page) {
    return [
        '//------------------------------------------------------------------------------',
        '// 此文件由 FineUI.Core 设计时工具自动生成。',
        '// 重新生成会覆盖手工修改。',
        '// 在 .cshtml 中加入 //NoRazorForms 可禁止生成此文件。',
        '//------------------------------------------------------------------------------',
        '',
        `namespace ${page.namespaceName}`,
        '{',
        `\tpublic partial class ${page.className}`,
        '\t{',
        ...page.controls.map((control) => `\t\tprotected FineUI.Core.${control.typeName} ${control.id};`),
        '\t}',
        '}',
        '',
    ].join('\r\n');
}

let checked = 0;
let changed = 0;
for (const pagePath of listFiles(repositoryRoot)) {
    const page = parsePage(readUtf8(pagePath), pagePath);
    if (!page) continue;
    const designerPath = `${pagePath}.designer.cs`;
    const expected = renderDesigner(page);
    const current = existsSync(designerPath) ? readUtf8(designerPath) : '';
    checked += 1;
    if (current.replace(/\r\n/g, '\n') === expected.replace(/\r\n/g, '\n')) continue;
    changed += 1;
    if (!checkOnly) writeFileSync(designerPath, expected, 'utf8');
    console.log(`${checkOnly ? '需要更新' : '已生成'}：${path.relative(repositoryRoot, designerPath)}`);
}

if (checkOnly && changed) {
    throw new Error(`有 ${changed} 个设计时文件需要重新生成。`);
}
console.log(`设计时文件检查完成：扫描 ${checked} 个页面，${checkOnly ? '待更新' : '已更新'} ${changed} 个。`);
