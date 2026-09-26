/**
 * FineUI 主题生成与监听工具。
 * 本文件由项目同步脚本生成；更新会覆盖手工修改。
 */
import fs from 'node:fs';
import path from 'node:path';
import { createInterface } from 'node:readline';
import { fileURLToPath } from 'node:url';

const themesDir = path.dirname(fileURLToPath(import.meta.url));
const watchChanges = process.argv.includes('--watch');
const targetThemes = process.argv.slice(2).filter(argument => argument !== '--watch');
const colors = { brand: 96, accent: 95, success: 92, update: 93, error: 91, hint: 90, subtitle: 97 };
// 每个字母使用 5×7 像素；两个方块字符拼成一个近似正方形的像素。
const logoGlyphs = {
    F: ['11111', '10000', '10000', '11110', '10000', '10000', '10000'],
    i: ['00100', '00000', '01100', '00100', '00100', '00100', '01110'],
    n: ['00000', '00000', '11110', '10001', '10001', '10001', '10001'],
    e: ['00000', '00000', '01110', '10001', '11111', '10000', '01111'],
    U: ['10001', '10001', '10001', '10001', '10001', '10001', '01110'],
    I: ['11111', '00100', '00100', '00100', '00100', '00100', '11111'],
};

// ============================================================
// 颜色工具函数
// ============================================================

function hexToRgb(hex) {
    hex = hex.replace('#', '');
    if (hex.length === 3) hex = hex[0] + hex[0] + hex[1] + hex[1] + hex[2] + hex[2];
    return {
        r: parseInt(hex.substring(0, 2), 16),
        g: parseInt(hex.substring(2, 4), 16),
        b: parseInt(hex.substring(4, 6), 16)
    };
}

function rgbToHex(r, g, b) {
    const toHex = (n) => {
        const h = Math.round(Math.max(0, Math.min(255, n))).toString(16);
        return h.length === 1 ? '0' + h : h;
    };
    return '#' + toHex(r) + toHex(g) + toHex(b);
}

/** 复刻 Sass color.scale($color, $lightness: percentage) */
function colorScale(hexColor, lightnessPercent) {
    const { h, s, l } = rgbToHsl(hexToRgb(hexColor));
    let newL;
    if (lightnessPercent >= 0) {
        newL = l + (100 - l) * (lightnessPercent / 100);
    } else {
        newL = l + l * (lightnessPercent / 100);
    }
    newL = Math.max(0, Math.min(100, newL));
    const rgb = hslToRgb(h, s, newL);
    return rgbToHex(rgb.r, rgb.g, rgb.b);
}

function rgbToHsl({ r, g, b }) {
    r /= 255; g /= 255; b /= 255;
    const max = Math.max(r, g, b), min = Math.min(r, g, b);
    let h, s, l = (max + min) / 2;
    if (max === min) {
        h = s = 0;
    } else {
        const d = max - min;
        s = l > 0.5 ? d / (2 - max - min) : d / (max + min);
        switch (max) {
            case r: h = ((g - b) / d + (g < b ? 6 : 0)) / 6; break;
            case g: h = ((b - r) / d + 2) / 6; break;
            case b: h = ((r - g) / d + 4) / 6; break;
        }
    }
    return { h: h * 360, s: s * 100, l: l * 100 };
}

function hslToRgb(h, s, l) {
    h /= 360; s /= 100; l /= 100;
    let r, g, b;
    if (s === 0) {
        r = g = b = l;
    } else {
        const hue2rgb = (p, q, t) => {
            if (t < 0) t += 1;
            if (t > 1) t -= 1;
            if (t < 1 / 6) return p + (q - p) * 6 * t;
            if (t < 1 / 2) return q;
            if (t < 2 / 3) return p + (q - p) * (2 / 3 - t) * 6;
            return p;
        };
        const q = l < 0.5 ? l * (1 + s) : l + s - l * s;
        const p = 2 * l - q;
        r = hue2rgb(p, q, h + 1 / 3);
        g = hue2rgb(p, q, h);
        b = hue2rgb(p, q, h - 1 / 3);
    }
    return { r: r * 255, g: g * 255, b: b * 255 };
}

function grayscale(hexColor) {
    const { h, s, l } = rgbToHsl(hexToRgb(hexColor));
    const rgb = hslToRgb(h, 0, l);
    return rgbToHex(rgb.r, rgb.g, rgb.b);
}

/** 复刻 _base.scss 的 lightness() 函数 */
function lightness(hexColor, lightnessValue, isDark = false, darkLightnessValue = false) {
    let resolvedValue = lightnessValue;
    if (isDark) {
        resolvedValue = (typeof darkLightnessValue === 'number') ? darkLightnessValue : -lightnessValue;
    }
    return colorScale(hexColor, resolvedValue);
}


// ============================================================
// 配置文件解析
// ============================================================

/**
 * 解析 theme.config 文件（key = value 格式）
 * 返回与 JSON 格式相同的对象
 */
function parseConfigFile(content) {
    const config = {};
    for (const line of content.split('\n')) {
        const trimmed = line.trim();
        // 跳过空行和注释
        if (!trimmed || trimmed.startsWith('#')) continue;
        const eqIndex = trimmed.indexOf('=');
        if (eqIndex === -1) continue;
        const key = trimmed.substring(0, eqIndex).trim();
        let value = trimmed.substring(eqIndex + 1).trim();
        // 布尔值转换
        if (value === 'true') value = true;
        else if (value === 'false') value = false;
        config[key] = value;
    }
    return config;
}


// ============================================================
// 从配置计算所有变量
// ============================================================

/**
 * 从 theme.config 解析出的配置对象计算所有 CSS 变量
 * 输入是 theme.config 解析后的主题名、颜色、尺寸和组件开关。
 */
function computeThemeVars(themeName, config) {
    const c = config; // 简写

    const isDark = c['is-dark-background'] || false;
    const isDarkActiveColor = c['is-dark-active-color'] !== undefined ? c['is-dark-active-color'] : true;

    // 核心颜色
    const coreColors = {};
    const coreNames = [
        'content-border-color', 'content-background-color', 'content-text-color',
        'header-border-color', 'header-background-color', 'header-text-color',
        'default-border-color', 'default-background-color', 'default-text-color',
        'hover-border-color', 'hover-background-color', 'hover-text-color',
        'active-border-color', 'active-background-color', 'active-text-color',
        'error-border-color', 'error-background-color', 'error-text-color',
        'border-radius'
    ];
    for (const name of coreNames) {
        if (c[name]) coreColors[name] = c[name];
    }

    // 派生变量
    const derived = {};

    derived['primary-background-color'] = c['primary-background-color'] || coreColors['active-background-color'];
    derived['primary-text-color'] = c['primary-text-color'] || coreColors['active-text-color'];
    derived['tabstrip-inkbar-color'] = c['tabstrip-inkbar-color'] || derived['primary-background-color'];

    const alphaStr = c['focus-shadow-alpha'] || '.15';
    derived['focus-shadow-alpha'] = alphaStr;
    derived['focus-shadow-percent'] = (parseFloat(alphaStr) * 100) + '%';

    // 可选 header hover/active
    const optionalVars = [
        'header-hover-border-color', 'header-hover-background-color', 'header-hover-text-color',
        'header-active-border-color', 'header-active-background-color', 'header-active-text-color'
    ];
    for (const name of optionalVars) {
        if (c[name]) derived[name] = c[name];
    }

    // index 系列
    const indexVars = [
        'index-header-border-color', 'index-header-background-color', 'index-header-text-color',
        'index-hover-border-color', 'index-hover-background-color', 'index-hover-text-color',
        'index-active-border-color', 'index-active-background-color', 'index-active-text-color'
    ];
    // 默认值
    const indexDefaults = {
        'index-header-border-color': coreColors['header-border-color'],
        'index-header-background-color': coreColors['header-background-color'],
        'index-header-text-color': coreColors['header-text-color'],
        'index-hover-border-color': coreColors['hover-border-color'],
        'index-hover-background-color': coreColors['hover-background-color'],
        'index-hover-text-color': coreColors['hover-text-color'],
        'index-active-border-color': coreColors['active-border-color'],
        'index-active-background-color': coreColors['active-background-color'],
        'index-active-text-color': coreColors['active-text-color']
    };
    for (const name of indexVars) {
        derived[name] = c[name] || indexDefaults[name];
    }

    // 预计算 lightness 派生变量
    const precomputed = {};
    const contentBg = coreColors['content-background-color'];
    const contentBorder = coreColors['content-border-color'];
    const contentText = coreColors['content-text-color'];
    const hoverBg = coreColors['hover-background-color'];
    const activeBg = coreColors['active-background-color'];
    const defaultBg = coreColors['default-background-color'];
    const primaryBg = derived['primary-background-color'];

    if (contentBg && contentBg.startsWith('#')) {
        precomputed['content-bg-darken-1'] = lightness(contentBg, -1, isDark);
        precomputed['content-bg-darken-4'] = lightness(contentBg, -4, isDark, 12);
        precomputed['content-bg-darken-5'] = lightness(contentBg, -5, isDark, 13);
        precomputed['content-bg-darken-5-d20'] = lightness(contentBg, -5, isDark, 20);
        precomputed['content-bg-darken-10'] = lightness(contentBg, -10, isDark);
        precomputed['content-bg-darken-30'] = lightness(contentBg, -30, isDark);
        precomputed['content-bg-darken-35'] = lightness(contentBg, -35, isDark);
        precomputed['content-bg-darken-40'] = lightness(contentBg, -40, isDark);
    }
    if (contentBorder && contentBorder.startsWith('#')) {
        precomputed['content-border-lighten-5'] = lightness(contentBorder, 5);
        precomputed['content-border-lighten-15'] = lightness(contentBorder, 15);
    }
    if (hoverBg && hoverBg.startsWith('#')) {
        precomputed['hover-bg-darken-5'] = lightness(hoverBg, -5, isDark, 5);
    }
    if (activeBg && activeBg.startsWith('#')) {
        precomputed['active-bg-darken-5'] = lightness(activeBg, -5, isDark, 5);
    }
    if (defaultBg && defaultBg.startsWith('#')) {
        precomputed['default-bg-darken-15'] = lightness(defaultBg, -15, isDark, 20);
    }
    if (primaryBg && primaryBg.startsWith('#')) {
        precomputed['primary-bg-dark'] = lightness(primaryBg, -20);
        precomputed['primary-bg-hover'] = lightness(primaryBg, -15);
    }
    if (contentText && contentText.startsWith('#')) {
        precomputed['checkbox-switch-color'] = lightness(grayscale(contentText), 40, isDark);
    }

    // 条件分支
    const conditional = {};
    if (isDark) {
        if (contentBorder && contentBorder.startsWith('#')) {
            conditional['locked-left-shadow'] = `inset 10px 0 8px -8px ${lightness(contentBorder, 15)}`;
            conditional['locked-right-shadow'] = `inset -10px 0 8px -8px ${lightness(contentBorder, 15)}`;
            conditional['center-tabstrip-shadow'] = `0 1px 1px 0 ${lightness(contentBorder, 5)}`;
            conditional['sidebar-shadow'] = `1px 0 1px 0 ${lightness(contentBorder, 5)}`;
            conditional['sidebar-left-shadow'] = `1px 0 1px 0 ${lightness(contentBorder, 5)}`;
        }
        conditional['radiobutton-dot-color'] = contentText;
    }
    if (!isDarkActiveColor) {
        if (primaryBg && primaryBg.startsWith('#')) {
            const primaryDark = lightness(primaryBg, -20);
            // unchecked checkbox/radio - 与非 active 行外观一致
            //   bg = content-bg（基础 .f-checkbox.f-icon 即此色）
            //   border = 显式 color-mix(content-text 60%)（与基础 .f-checkbox.f-icon 的 border-color 公式一致）
            if (contentBg) {
                conditional['active-checkbox-bg'] = contentBg;
            }
            conditional['active-checkbox-border'] = 'color-mix(in srgb, var(--f-content-text-color) 60%, transparent)';
            // checked checkbox - primaryDark 柔和染色
            conditional['active-checkbox-checked-bg'] = primaryDark;
            conditional['active-checkbox-checked-border'] = primaryDark;
            // radio / switch - 都按 checked 染色（unchecked radio 复用上面 checkbox 的 unchecked 规则）
            conditional['active-radio-bg'] = primaryDark;
            conditional['active-radio-border'] = primaryDark;
            conditional['active-switch-bg'] = primaryDark;
            conditional['active-switch-border'] = primaryDark;
            // progressbar - 整条 bar 用基础背景色（content-bg-darken-10），只 value 段染色
            conditional['active-progressbar-bg'] = 'var(--f-content-bg-darken-10)';
            conditional['active-progressbar-value-bg'] = primaryDark;
            // rate - 满/半星染色
            conditional['active-rate-color'] = primaryDark;
        }
    }

    return {
        themeName,
        coreColors,
        isDark,
        isDarkActiveColor,
        derived,
        precomputed,
        conditional,
        extraCss: '' // 额外 CSS 从 theme-extra.css 文件读取
    };
}


// ============================================================
// CSS 输出
// ============================================================

function generateThemeVarsCss(themeData) {
    const lines = [];
    lines.push(`/* FineUI 主题变量 - ${themeData.themeName} */`);
    lines.push(`/* 由 generate-theme.js 自动生成，请勿手动修改 */`);
    lines.push('');
    lines.push(':root {');

    lines.push('    /* 核心颜色 */');
    const coreOrder = [
        'content-border-color', 'content-background-color', 'content-text-color',
        'header-border-color', 'header-background-color', 'header-text-color',
        'default-border-color', 'default-background-color', 'default-text-color',
        'hover-border-color', 'hover-background-color', 'hover-text-color',
        'active-border-color', 'active-background-color', 'active-text-color',
        'error-border-color', 'error-background-color', 'error-text-color',
        'border-radius'
    ];
    for (const name of coreOrder) {
        if (themeData.coreColors[name]) {
            lines.push(`    --f-${name}: ${themeData.coreColors[name]};`);
        }
    }

    lines.push('');
    lines.push('    /* 派生变量 */');
    const derivedOrder = [
        'primary-background-color', 'primary-text-color', 'tabstrip-inkbar-color',
        'focus-shadow-alpha', 'focus-shadow-percent',
        'header-hover-border-color', 'header-hover-background-color', 'header-hover-text-color',
        'header-active-border-color', 'header-active-background-color', 'header-active-text-color',
        'index-header-border-color', 'index-header-background-color', 'index-header-text-color',
        'index-hover-border-color', 'index-hover-background-color', 'index-hover-text-color',
        'index-active-border-color', 'index-active-background-color', 'index-active-text-color'
    ];
    for (const name of derivedOrder) {
        if (themeData.derived[name] !== undefined) {
            lines.push(`    --f-${name}: ${themeData.derived[name]};`);
        }
    }

    lines.push('');
    lines.push('    /* 预计算颜色 */');
    for (const [name, value] of Object.entries(themeData.precomputed)) {
        lines.push(`    --f-${name}: ${value};`);
    }

    if (Object.keys(themeData.conditional).length > 0) {
        lines.push('');
        lines.push('    /* 条件分支覆盖 */');
        for (const [name, value] of Object.entries(themeData.conditional)) {
            lines.push(`    --f-${name}: ${value};`);
        }
    }

    lines.push('}');

    if (themeData.extraCss) {
        lines.push('');
        lines.push('/* 主题特有样式 */');
        lines.push(themeData.extraCss);
    }

    lines.push('');
    return lines.join('\n');
}


// ============================================================
// 主题目录处理
// ============================================================

/**
 * 处理一个主题目录：读取 theme.config，生成 theme.css
 */
// 只在内容真的变了才写文件。两个用途：
//   ① 主题生成：theme.css 内容没变就不碰它，用户的文件 mtime 保持稳定；
//   ② 同步链路：目标文件靠 (size, mtime) 判断是否跳过（见 lib.mjs 的 skipUnchanged），
//      这里白写一次，下一轮同步就白拷一次。
// 返回是否真的写了。
function writeIfChanged(filePath, content) {
    if (fs.existsSync(filePath)) {
        try {
            if (fs.readFileSync(filePath, 'utf-8') === content) return false;
        } catch (_) { /* 读不了就当需要重写 */ }
    }
    fs.writeFileSync(filePath, content, 'utf-8');
    return true;
}

function processThemeDir(themesDir, themeName) {
    const configPath = path.join(themesDir, themeName, 'theme.config');
    const config = parseConfigFile(fs.readFileSync(configPath, 'utf-8'));
    const themeData = computeThemeVars(themeName, config);

    // 读取可选的 theme-extra.css
    const extraCssPath = path.join(themesDir, themeName, 'theme-extra.css');
    if (fs.existsSync(extraCssPath)) {
        themeData.extraCss = fs.readFileSync(extraCssPath, 'utf-8').trim();
    }

    const css = generateThemeVarsCss(themeData);

    writeIfChanged(path.join(themesDir, themeName, 'theme.css'), css);

    const varCount = (css.match(/--f-/g) || []).length;
    return varCount;
}

function paint(message, color, stream = process.stdout) {
    const useColor = stream.isTTY && !('NO_COLOR' in process.env) && process.env.TERM !== 'dumb';
    return useColor ? `\u001b[${color}m${message}\u001b[0m` : message;
}

function printStatus(symbol, title, detail, color, stream = process.stdout) {
    stream.write(`${paint(`${symbol} ${title}`, color, stream)}  ${detail}\n`);
}

function printLogo() {
    if (!process.stdout.isTTY) return;

    process.stdout.write('\n');
    if (process.stdout.columns && process.stdout.columns < 76) {
        process.stdout.write(`  ${paint('◆ FineUI', colors.brand)}\n`);
    } else {
        for (let row = 0; row < 7; row += 1) {
            let line = '  ';
            for (const [index, letter] of [...'FineUI'].entries()) {
                const pixels = logoGlyphs[letter][row].replace(/1/g, '██').replace(/0/g, '  ');
                line += `${paint(pixels, index < 4 ? colors.brand : colors.accent)}  `;
            }
            process.stdout.write(`${line}\n`);
        }
    }
    process.stdout.write(`  ${paint('主题生成与监听', colors.subtitle)}\n\n`);
}

function isThemeDirectory(name) {
    const directory = path.join(themesDir, name);
    return fs.existsSync(directory) && fs.statSync(directory).isDirectory();
}

function themeNames() {
    const names = fs.readdirSync(themesDir);
    return names.filter(name =>
        (targetThemes.length === 0 || name === targetThemes[0]) && isThemeDirectory(name)
    ).sort();
}

function generateTheme(name) {
    const configPath = path.join(themesDir, name, 'theme.config');
    if (!fs.existsSync(configPath)) return { checked: 0, created: 0, updated: 0, failed: 0 };

    const outputPath = path.join(themesDir, name, 'theme.css');
    try {
        const existed = fs.existsSync(outputPath);
        const previousCss = existed ? fs.readFileSync(outputPath, 'utf8') : null;
        const varCount = processThemeDir(themesDir, name);
        const currentCss = fs.readFileSync(outputPath, 'utf8');
        if (currentCss !== previousCss) {
            const action = existed ? '已更新' : '已生成';
            const symbol = existed ? '~' : '+';
            const color = existed ? colors.update : colors.success;
            printStatus(`  ${symbol}`, action, `${name}/theme.css · ${varCount} 个变量`, color);
        }
        return {
            checked: 1,
            created: existed ? 0 : 1,
            updated: existed && currentCss !== previousCss ? 1 : 0,
            failed: 0,
        };
    } catch (error) {
        printStatus('!', '生成失败', `${name}: ${error.message}`, colors.error, process.stderr);
        return { checked: 1, created: 0, updated: 0, failed: 1 };
    }
}

function generateAll() {
    const totals = { checked: 0, created: 0, updated: 0, failed: 0 };
    printStatus('◆', '扫描主题', '正在查找 theme.config…', colors.brand);

    for (const name of themeNames()) {
        const result = generateTheme(name);
        for (const key of Object.keys(totals)) totals[key] += result[key];
    }

    if (targetThemes.length === 1 && totals.checked === 0) {
        printStatus('!', '未找到配置', `${targetThemes[0]}/theme.config`, colors.error, process.stderr);
        totals.failed += 1;
    }

    printStatus(
        '★',
        '检查完成',
        `${totals.checked} 个主题 · 新建 ${totals.created} 个 · 更新 ${totals.updated} 个 · 失败 ${totals.failed} 个`,
        totals.failed ? colors.error : colors.success,
    );
    return totals;
}

function startWatching() {
    const watchers = new Map();
    const timers = new Map();

    function scheduleGeneration(name) {
        clearTimeout(timers.get(name));
        timers.set(name, setTimeout(() => {
            timers.delete(name);
            generateTheme(name);
        }, 120));
    }

    function syncWatchers() {
        const names = new Set(themeNames());
        for (const [name, watcher] of watchers) {
            if (!names.has(name)) {
                watcher.close();
                watchers.delete(name);
                clearTimeout(timers.get(name));
                timers.delete(name);
            }
        }

        for (const name of names) {
            if (watchers.has(name)) continue;
            try {
                const watcher = fs.watch(path.join(themesDir, name), (event, filename) => {
                    if (!filename || filename === 'theme.config' || filename === 'theme-extra.css') {
                        scheduleGeneration(name);
                    }
                });
                watcher.on('error', error => {
                    printStatus('!', '监听失败', `${name}: ${error.message}`, colors.error, process.stderr);
                });
                watchers.set(name, watcher);
                if (fs.existsSync(path.join(themesDir, name, 'theme.config'))) {
                    scheduleGeneration(name);
                }
            } catch (error) {
                printStatus('!', '监听失败', `${name}: ${error.message}`, colors.error, process.stderr);
            }
        }
    }

    syncWatchers();
    // 已存在的主题在进入监听前扫描过，取消初次注册时安排的重复生成。
    for (const timer of timers.values()) clearTimeout(timer);
    timers.clear();

    const rootWatcher = fs.watch(themesDir, () => syncWatchers());
    rootWatcher.on('error', error => {
        printStatus('!', '监听失败', error.message, colors.error, process.stderr);
    });
    printStatus('●', '正在监听', 'theme.config 与 theme-extra.css；按回车停止，或直接关闭窗口。', colors.brand);

    const input = createInterface({ input: process.stdin, output: process.stdout });
    function stop() {
        rootWatcher.close();
        for (const watcher of watchers.values()) watcher.close();
        for (const timer of timers.values()) clearTimeout(timer);
        input.close();
        process.stdin.pause();
        printStatus('○', '已停止监听', '主题文件保持当前状态。', colors.hint);
        // Windows 的管道输入在 readline 关闭后仍可能占住事件循环。
        setImmediate(() => process.exit(0));
    }
    input.once('line', stop);
    process.once('SIGINT', stop);
}

if (targetThemes.length > 1 || process.argv.slice(2).some(argument => argument.startsWith('--') && argument !== '--watch')) {
    printStatus('!', '参数错误', '用法：node generate-theme.mjs [--watch] [主题名]', colors.error, process.stderr);
    process.exitCode = 2;
} else {
    printLogo();
    const result = generateAll();
    if (watchChanges) startWatching();
    else if (result.failed) process.exitCode = 1;
}
