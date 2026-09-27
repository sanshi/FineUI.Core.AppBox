import { existsSync, readdirSync, readFileSync, statSync, watch, writeFileSync } from 'node:fs';
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
        }
    }
    return result;
}

function readUtf8(filePath) {
    return decoder.decode(readFileSync(filePath));
}

function parsePage(source, filePath) {
    if (source.includes('//NoRazorForms')) {
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
        if (separator < 1) {
            throw new Error(`${filePath} 的 @model 和代码后置文件都无法确定完整类型。`);
        }

        namespaceName = fullModel.slice(0, separator);
        className = fullModel.slice(separator + 1);
    }
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

function generatePage(pagePath) {
    if (!existsSync(pagePath)) {
        return { checked: 0, created: 0, updated: 0 };
    }

    const page = parsePage(readUtf8(pagePath), pagePath);
    if (!page) {
        return { checked: 0, created: 0, updated: 0 };
    }

    const designerPath = `${pagePath}.designer.cs`;
    const expected = renderDesigner(page);
    const designerExists = existsSync(designerPath);
    const current = designerExists ? readUtf8(designerPath) : '';

    if (current.replace(/\r\n/g, '\n') === expected.replace(/\r\n/g, '\n')) {
        return { checked: 1, created: 0, updated: 0 };
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
    };
}

function generateAllPages() {
    let checked = 0;
    let created = 0;
    let updated = 0;

    printStatus('◆', checkOnly ? '检查页面' : '扫描页面', '正在查找 RazorForms 页面…', colors.brand);

    for (const pagePath of listFiles(repositoryRoot)) {
        const result = generatePage(pagePath);
        checked += result.checked;
        created += result.created;
        updated += result.updated;
    }

    const createdLabel = checkOnly ? '待生成' : '新建';
    const updatedLabel = checkOnly ? '待更新' : '更新';
    printStatus(
        '★',
        checkOnly ? '检查完成' : '扫描完成',
        `${checked} 个页面 · ${createdLabel} ${created} 个 · ${updatedLabel} ${updated} 个`,
        colors.success,
    );

    const pendingChanges = created + updated;
    if (checkOnly && pendingChanges > 0) {
        printStatus('▲', '检查未通过', '请运行生成器更新设计时文件。', colors.update, process.stderr);
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

        const pagePath = path.resolve(repositoryRoot, filename.toString());
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
    printStatus('◆', '监听中', '保存 .cshtml 后自动更新对应的 .cshtml.designer.cs 文件。', colors.accent);
    printStatus('·', '退出方式', '按回车停止监听，或直接关闭窗口。', colors.hint);
}

printLogo();
generateAllPages();
if (watchChanges) {
    watchPages();
}
