import { existsSync, readdirSync, readFileSync, statSync, unlinkSync, watch, writeFileSync } from 'node:fs';
import path from 'node:path';
import { createInterface } from 'node:readline';
import { fileURLToPath } from 'node:url';

const toolDirectory = path.dirname(fileURLToPath(import.meta.url));
const repositoryRoot = path.resolve(toolDirectory, '..');
const checkOnly = process.argv.includes('--check');
const watchChanges = process.argv.includes('--watch');
const decoder = new TextDecoder('utf-8', { fatal: true });
const ignoredDirectories = new Set(['.git', 'bin', 'obj', 'packages', 'node_modules']);
const controlTypeAliases = new Map([
    ['ContentPanel', 'Panel'],
]);
const generatedFileNotice = '// 此文件由 FineUI.Core 设计时工具自动生成。';
const logo = JSON.parse(readFileSync(new URL('./console-logo.json', import.meta.url), 'utf8'));

function hexColorToAnsi(hexColor) {
    const red = parseInt(hexColor.slice(1, 3), 16);
    const green = parseInt(hexColor.slice(3, 5), 16);
    const blue = parseInt(hexColor.slice(5, 7), 16);
    return `38;2;${red};${green};${blue}`;
}

const colors = {
    brand: hexColorToAnsi(logo.primaryColor),
    accent: hexColorToAnsi(logo.accentColor),
    success: 92,
    update: 93,
    error: 91,
    hint: 90,
    subtitle: 97,
};
if (checkOnly && watchChanges) {
    throw new Error('--check 和 --watch 不能同时使用。');
}

function paint(text, color, stream = process.stdout) {
    const useColor = stream.isTTY
        && !('NO_COLOR' in process.env)
        && process.env.TERM !== 'dumb';
    return useColor ? `\u001b[${color}m${text}\u001b[0m` : text;
}

function printStatus(symbol, title, detail, color, stream = process.stdout) {
    const prefix = paint(`${symbol} ${title}`, color, stream);
    stream.write(`${prefix}  ${detail}\n`);
}

function printLogo() {
    if (!process.stdout.isTTY) {
        return;
    }

    process.stdout.write('\n');
    if (process.stdout.columns && process.stdout.columns < 76) {
        // 窄终端使用短标识，避免像素字标自动折行。
        process.stdout.write(`  ${paint('◆ FineUI', colors.brand)}\n`);
    } else {
        for (let row = 0; row < 7; row += 1) {
            let line = '  ';
            for (const [index, letter] of [...logo.text].entries()) {
                const glyphKey = letter === 'I' ? 'CapitalI' : letter;
                const pixels = logo.glyphs[glyphKey][row]
                    .replace(/1/g, '██')
                    .replace(/0/g, '  ');
                const letterColor = index < logo.primaryLetterCount ? colors.brand : colors.accent;
                line += `${paint(pixels, letterColor)}  `;
            }
            process.stdout.write(`${line}\n`);
        }
    }

    process.stdout.write(`  ${paint('RazorForms · 设计时文件生成与监听', colors.subtitle)}\n\n`);
}

function listFiles(directory, result = []) {
    for (const name of readdirSync(directory)) {
        if (ignoredDirectories.has(name)) {
            continue;
        }

        const filePath = path.join(directory, name);
        if (statSync(filePath).isDirectory()) {
            listFiles(filePath, result);
        } else if (name.endsWith('.cshtml')) {
            result.push(filePath);
        } else if (name.endsWith('.cshtml.designer.cs')) {
            // 页面已被删除时，也检查它遗留的生成文件。
            result.push(filePath.slice(0, -'.designer.cs'.length));
        }
    }
    return result;
}

function readUtf8(filePath) {
    return decoder.decode(readFileSync(filePath));
}

function parseControls(source, filePath) {
    const controls = [];
    const seenIds = new Map();
    const tagPattern = /<f:([A-Za-z_][A-Za-z0-9_.]*)\b([^>]*)>/g;
    for (const match of source.matchAll(tagPattern)) {
        const idMatch = match[2].match(/\bID\s*=\s*"([A-Za-z_][A-Za-z0-9_]*)"/i);
        if (!idMatch) {
            continue;
        }

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
    return controls;
}

function parsePage(source, filePath) {
    // 兼容已有页面的排除标记；普通页面不需要添加标记。
    if (/\/\/\s*NoRazorForms\b|@\*\s*NoRazorForms\s*\*@/.test(source)) {
        return null;
    }

    const modelLine = source.match(/^\s*@model\s+([^\r\n]+)/m)?.[1] ?? '';
    if (modelLine.includes('<')) {
        return null;
    }

    const modelMatch = source.match(/^\s*@model\s+([A-Za-z_][A-Za-z0-9_.]*)/m);
    if (!modelMatch) {
        return null;
    }

    const controls = parseControls(source, filePath);
    if (controls.length === 0) {
        // designer 只声明控件字段，下载、数据接口等页面不需要空类。
        return null;
    }

    // @model 只决定 Razor 运行时类型；历史页面可能保留了过时值。
    // designer 必须与同页 code-behind 的 partial class 合并，因此优先读取后者。
    const codeBehindPath = `${filePath}.cs`;
    const codeBehindExists = existsSync(codeBehindPath);
    const codeBehind = codeBehindExists ? readUtf8(codeBehindPath) : '';
    const namespaceMatch = codeBehind.match(/\bnamespace\s+([A-Za-z_][A-Za-z0-9_.]*)/);
    const classMatch = codeBehind.match(/\bpartial\s+class\s+([A-Za-z_][A-Za-z0-9_]*)/);
    if (codeBehindExists && !classMatch) {
        throw new Error(`${codeBehindPath} 的页面模型必须声明为 partial class，才能合并自动生成的控件字段。`);
    }

    let namespaceName;
    let className;
    if (namespaceMatch && classMatch) {
        namespaceName = namespaceMatch[1];
        className = classMatch[1];
    } else {
        const fullModel = modelMatch[1];
        const separator = fullModel.lastIndexOf('.');
        if (separator < 1) {
            throw new Error(`${filePath} 的 @model 和代码后置文件都无法确定完整类型。`);
        }

        namespaceName = fullModel.slice(0, separator);
        className = fullModel.slice(separator + 1);
    }
    return { namespaceName, className, controls };
}

function renderDesigner(page) {
    return [
        generatedFileNotice,
        '// 重新生成会覆盖手工修改。',
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

function isGeneratedDesigner(source) {
    if (!source.includes(generatedFileNotice)) {
        return false;
    }

    // 同时核对文件头和类结构，保留来源不明或被加入手写成员的文件。
    const body = source.replace(/^\s*\/\/[^\r\n]*/gm, '').trim();
    const identifierPattern = '[A-Za-z_][A-Za-z0-9_]*';
    const qualifiedNamePattern = `${identifierPattern}(?:\\.${identifierPattern})*`;
    const fieldPattern = `protected\\s+FineUI\\.Core\\.${qualifiedNamePattern}\\s+${identifierPattern};`;
    const generatedClassPattern = new RegExp([
        `^namespace\\s+${qualifiedNamePattern}\\s*\\{\\s*`,
        `public\\s+partial\\s+class\\s+${identifierPattern}\\s*\\{\\s*`,
        `(?:${fieldPattern}\\s*)*`,
        '\\}\\s*\\}$',
    ].join(''));
    return generatedClassPattern.test(body);
}

function removeGeneratedDesigner(designerPath) {
    const unchanged = { checked: 0, created: 0, updated: 0, removed: 0 };
    if (!existsSync(designerPath)) {
        return unchanged;
    }

    const relativePath = path.relative(repositoryRoot, designerPath);
    if (relativePath === '..' || relativePath.startsWith(`..${path.sep}`) || path.isAbsolute(relativePath)) {
        throw new Error(`不能清理项目目录之外的文件：${designerPath}`);
    }

    if (!isGeneratedDesigner(readUtf8(designerPath))) {
        printStatus('  ·', '保留文件', `无法确认是本工具生成的文件，请自行检查：${relativePath}`, colors.hint);
        return unchanged;
    }

    if (!checkOnly) {
        unlinkSync(designerPath);
    }

    printStatus('  -', checkOnly ? '待删除' : '已删除', relativePath, colors.update);
    return { checked: 1, created: 0, updated: 0, removed: 1 };
}

function generatePage(pagePath) {
    const designerPath = `${pagePath}.designer.cs`;
    const page = existsSync(pagePath) ? parsePage(readUtf8(pagePath), pagePath) : null;
    if (!page) {
        return removeGeneratedDesigner(designerPath);
    }

    const expected = renderDesigner(page);
    const designerExists = existsSync(designerPath);
    const current = designerExists ? readUtf8(designerPath) : '';

    if (current.replace(/\r\n/g, '\n') === expected.replace(/\r\n/g, '\n')) {
        return { checked: 1, created: 0, updated: 0, removed: 0 };
    }

    if (!checkOnly) {
        writeFileSync(designerPath, expected, 'utf8');
    }

    const action = designerExists ? '更新' : '生成';
    const actionState = checkOnly ? '待' : '已';
    const symbol = designerExists ? '~' : '+';
    const color = designerExists ? colors.update : colors.success;
    printStatus(`  ${symbol}`, `${actionState}${action}`, path.relative(repositoryRoot, designerPath), color);

    return {
        checked: 1,
        created: designerExists ? 0 : 1,
        updated: designerExists ? 1 : 0,
        removed: 0,
    };
}

function generateAllPages() {
    let checked = 0;
    let created = 0;
    let updated = 0;
    let removed = 0;

    printStatus('◆', checkOnly ? '检查页面' : '扫描页面', '正在查找 RazorForms 页面…', colors.brand);

    for (const pagePath of new Set(listFiles(repositoryRoot))) {
        const result = generatePage(pagePath);
        checked += result.checked;
        created += result.created;
        updated += result.updated;
        removed += result.removed;
    }

    const createdLabel = checkOnly ? '待生成' : '新建';
    const updatedLabel = checkOnly ? '待更新' : '更新';
    const removedLabel = checkOnly ? '待删除' : '删除';
    printStatus(
        '★',
        checkOnly ? '检查完成' : '扫描完成',
        `${checked} 个页面 · ${createdLabel} ${created} 个 · ${updatedLabel} ${updated} 个 · ${removedLabel} ${removed} 个`,
        colors.success,
    );

    const pendingChanges = created + updated + removed;
    if (checkOnly && pendingChanges > 0) {
        printStatus('▲', '检查未通过', '请运行生成器更新或清理设计时文件。', colors.update, process.stderr);
        process.exitCode = 1;
    }
}

function watchPages() {
    const pendingUpdates = new Map();
    let fullScanTimer;
    const input = process.stdin.isTTY
        ? createInterface({ input: process.stdin, output: process.stdout })
        : null;

    function runUpdate(action) {
        try {
            action();
        } catch (error) {
            // 单个页面暂时无法读取时继续监听，下一次保存仍可重新生成。
            printStatus('■', '更新失败', error.message, colors.error, process.stderr);
        }
    }

    const watcher = watch(repositoryRoot, { recursive: true }, (_eventType, filename) => {
        if (!filename) {
            // 某些文件系统不返回文件名，只能重新扫描所有页面。
            clearTimeout(fullScanTimer);
            fullScanTimer = setTimeout(() => runUpdate(generateAllPages), 200);
            return;
        }

        const changedPath = path.resolve(repositoryRoot, filename.toString());
        const isCodeBehind = changedPath.endsWith('.cshtml.cs');
        const pagePath = isCodeBehind ? changedPath.slice(0, -'.cs'.length) : changedPath;
        const relativeParts = path.relative(repositoryRoot, pagePath).split(path.sep);
        const isRazorPage = pagePath.endsWith('.cshtml');
        const isIgnored = relativeParts.some((part) => ignoredDirectories.has(part));
        if (!isRazorPage || isIgnored) {
            return;
        }

        clearTimeout(pendingUpdates.get(pagePath));
        const timer = setTimeout(() => {
            pendingUpdates.delete(pagePath);
            runUpdate(() => generatePage(pagePath));
        }, 200);
        pendingUpdates.set(pagePath, timer);
    });

    watcher.on('error', (error) => {
        printStatus('■', '监听失败', error.message, colors.error, process.stderr);
        process.exitCode = 1;
        watcher.close();
        input?.close();
    });

    input?.once('line', () => {
        watcher.close();
        input.close();
        printStatus('★', '已停止', '设计时文件监听已结束。', colors.success);
    });

    process.stdout.write('\n');
    printStatus('◆', '监听中', '保存 .cshtml 或对应后台文件后，自动更新或清理设计时文件。', colors.accent);
    printStatus('·', '退出方式', '按回车停止监听，或直接关闭窗口。', colors.hint);
}

printLogo();
generateAllPages();
if (watchChanges) {
    watchPages();
}
