import { readdirSync, readFileSync } from 'node:fs';
import path from 'node:path';
import { fileURLToPath } from 'node:url';

const decoder = new TextDecoder('utf-8', { fatal: true });
const ignoredDirectories = new Set([
    '.git', '.vs', '.local', '.agents', 'bin', 'obj', 'packages', 'node_modules',
    '_out', 'tools', 'lib', 'vendor', 'third-party', 'third_party', 'themes'
]);
const sourceExtensions = new Set(['.cs', '.cshtml', '.js', '.html']);
const rules = [
    {
        id: 'page-mode-setting', category: 'protocol',
        pattern: /\bPageManager\s*\.\s*Instance\s*\.\s*(?:EnableRazorForms|EnableWebForms)\s*=|\bF\s*\.\s*PageManager\s*\.\s*(?:EnableRazorForms|EnableWebForms)\s*\(/,
        message: 'PageManager 每页模式设置已废弃；全局开启 RF 时，在原生页面的模型类或共同基类上添加 [NoRazorForms]。独立接口无需标记。'
    },
    {
        id: 'startup-script', category: 'strict-csp',
        pattern: /\b(?:Register(?:Pre|Post)?StartupScript|AddAjax(?:Absolute)?Script)\s*\(/,
        message: '普通策略保留旧脚本；严格 CSP 改用控件即时方法、PageContext.Execute 或具名函数。'
    },
    {
        id: 'script-reference', category: 'strict-csp',
        pattern: /\bGet[A-Za-z0-9_]*Reference\s*\(/,
        message: '脚本字符串引用在严格 CSP 下需迁为即时方法或 ClientCommand；不要把返回值传给 InvokeFunction。'
    },
    {
        id: 'script-callback', category: 'strict-csp',
        pattern: /\b(?:OkScript|CancelScript|HideScript|ScriptParameters|OnClientClick|HideExecuteScript)\s*(?:=|\()/,
        message: '核查字符串回调或表达式；消息回调可用 OkCommand、CancelCommand、OkFunction、CancelFunction。'
    },
    {
        id: 'inline-handler', category: 'strict-csp',
        pattern: /\b(?:href|src)\s*=\s*["']\s*javascript:/i,
        message: '内联事件和脚本网址不受 nonce 授权；改用模板中的具名函数与事件委托。'
    },
    {
        id: 'expression-handler', category: 'strict-csp',
        pattern: /\b(?:Handler|RendererFunction|ClickHandler)\s*=\s*["'][^"']*(?:\bfunction\b|=>|;)/,
        message: '函数属性中的函数正文或语句须改为模板具名函数；renderSalary(2) 这类 JSON 工厂参数可以保留。'
    },
    {
        id: 'message-overload', category: 'audit',
        pattern: /\b(?:Alert|Confirm|Prompt|Notify)\s*\.\s*Show\s*\(/,
        message: '核查消息重载是否传入脚本字符串回调；普通消息文字不需要迁移，延后动作可用命令或具名函数。'
    },
    {
        id: 'response-script', category: 'protocol',
        pattern: /(?:data|responseText|response|result)\s*\.(?:indexOf|includes|match|search|startsWith)\s*\(\s*["'][^"']*(?:location|window\.|F\.)/,
        message: 'Core UI 响应已是 JSON 文本；按 JSON.parse(data) 的 commands 判断跳转等动作。原脚本正文识别不兼容。'
    },
    {
        id: 'response-execution', category: 'protocol',
        pattern: /\b(?:eval|Function)\s*\(\s*(?:data|responseText|response|result)\b/,
        message: '不能继续执行 Core UI 响应正文；使用新客户端解释 JSON，核查自定义响应接管逻辑。'
    },
    {
        id: 'handler-url', category: 'audit',
        pattern: /[?&]handler=/i,
        message: '普通控件事件使用当前完整 URL；显式上传、下载、自定义 JSON 的原生 handler 应保留，勿批量替换。'
    },
    {
        id: 'data-source', category: 'audit',
        pattern: /\bDataSource\s*=\s*["']?@/,
        message: '回发会重跑模板并再次求值；将首次数据装载移到 Page_Load 的 !IsPostBack 分支，依赖 OnGet 或有副作用的表达式必须迁移。'
    },
    {
        id: 'custom-control', category: 'audit',
        pattern: /\bclass\s+\w+\s*:\s*(?:ControlBase|Component|BoxComponent|PanelBase|Field|Grid|Button)\b/,
        message: '自定义派生控件需要核查类型化创建与可信声明恢复；内置控件通过不代表自定义扩展已支持。'
    },
    {
        id: 'conditional-event', category: 'audit',
        pattern: /\b(?:Events|Listeners)\s*\.\s*(?:Add|Clear|Remove)\s*\(/,
        message: '核查条件与动态事件登记；每轮在 Page_Load 为已恢复的动态控件重新登记事件，不依赖客户端方法名。'
    },
    {
        id: 'raw-html-script', category: 'audit',
        pattern: /\bHtml\.Raw\s*\(|<script\b/i,
        message: '核查脚本来源与函数属性；可信 Razor 内联脚本可获 nonce，Html.Raw 和用户内容注入的脚本不能自动授权。'
    }
];

// 忽略代码注释而保留字符串，网址中的 // 仍属于字符串；行号始终与原文件一致。
function removeComments(source) {
    let result = '';
    let quote = '';
    let blockComment = false;
    let lineComment = false;
    for (let index = 0; index < source.length; index++) {
        const current = source[index];
        const next = source[index + 1];
        if (lineComment) {
            result += current === '\n' ? '\n' : ' ';
            lineComment = current !== '\n';
        } else if (blockComment) {
            if (current === '*' && next === '/') {
                result += '  ';
                index++;
                blockComment = false;
            } else {
                result += current === '\n' ? '\n' : ' ';
            }
        } else if (quote) {
            result += current;
            if (current === '\\' && next !== undefined) {
                result += next;
                index++;
            } else if (current === quote) {
                quote = '';
            }
        } else if (current === '/' && (next === '/' || next === '*')) {
            lineComment = next === '/';
            blockComment = next === '*';
            result += '  ';
            index++;
        } else {
            result += current;
            if (current === '"' || current === "'" || current === '`') {
                quote = current;
            }
        }
    }
    return result;
}

export function inspectUpgradeSource(source, file = '', mode = 'razorforms') {
    const uncommented = removeComments(source.replace(/^\uFEFF/, ''));
    const findings = [];
    // Razor 属性和 C# 调用可以跨行；例如 Handler="function(event) { 换行后才写正文。
    // 对完整源码匹配，再用命中位置换算原始行号，不能把模板压成一行后报告。
    for (const rule of rules) {
        if (mode === 'native' && ['data-source', 'conditional-event'].includes(rule.id)) {
            // 原生模式不重跑模板；标签 DataSource 和宿主处理器属于它原有的业务写法。
            continue;
        }
        const pattern = new RegExp(rule.pattern.source, rule.pattern.flags + 'g');
        for (const match of uncommented.matchAll(pattern)) {
            const line = uncommented.slice(0, match.index).split('\n').length;
            findings.push({ file, line, rule: rule.id, category: rule.category, message: rule.message });
        }
    }
    // HTML onclick 是浏览器执行的字符串；FineUI OnClick 是服务器事件名，不能要求客户把它删掉。
    // 按完整起始标签判断，也覆盖属性分多行写的普通 HTML。
    for (const match of uncommented.matchAll(/<([\w:-]+)\b((?:[^<>"']|"[^"]*"|'[^']*')*)>/g)) {
        if (/^f:/i.test(match[1])) {
            if (/^f:PageManager$/i.test(match[1])
                && /\b(?:EnableRazorForms|EnableWebForms)\s*=/i.test(match[2])) {
                const line = uncommented.slice(0, match.index).split('\n').length;
                findings.push({ file, line, rule: 'page-mode-setting', category: 'protocol',
                    message: '标签中的页面模式设置已废弃；请在原生页面的模型类或共同基类上添加 [NoRazorForms]。' });
            }
            continue;
        }
        for (const attribute of match[2].matchAll(/\bon[a-z]+\s*=\s*["']/gi)) {
            const position = match.index + match[0].indexOf(match[2]) + attribute.index;
            const line = uncommented.slice(0, position).split('\n').length;
            findings.push({ file, line, rule: 'inline-handler', category: 'strict-csp',
                message: 'HTML 内联事件不受 nonce 授权；改用模板中的具名函数与事件委托。' });
        }
    }
    findings.sort((left, right) => left.line - right.line);
    return findings;
}

export function scanUpgradeProject(projectRoot) {
    const root = path.resolve(projectRoot);
    const findings = [];
    function visit(directory) {
        for (const entry of readdirSync(directory, { withFileTypes: true }).sort((left, right) => left.name.localeCompare(right.name))) {
            const filename = path.join(directory, entry.name);
            // 不跟随符号链接，避免扫描到用户另一个项目或忽略目录中的副本。
            if (entry.isDirectory() && !ignoredDirectories.has(entry.name.toLowerCase())) {
                visit(filename);
            } else if (entry.isFile() && sourceExtensions.has(path.extname(entry.name))
                && !/\.designer\.cs$|\.min\.js$|^FineUI(?:_nojq)?\.js$/i.test(entry.name)) {
                const source = decoder.decode(readFileSync(filename));
                findings.push(...inspectUpgradeSource(source, path.relative(root, filename).replaceAll('\\', '/')));
            }
        }
    }
    visit(root);
    return findings;
}

function main(argumentsList) {
    const json = argumentsList.includes('--json');
    const paths = argumentsList.filter(argument => argument !== '--json');
    if (paths.length > 1 || paths.some(argument => argument.startsWith('--'))) {
        throw new Error('用法：node tools/check-razorforms-upgrade.mjs [项目目录] [--json]');
    }
    const projectRoot = paths[0] || process.cwd();
    const findings = scanUpgradeProject(projectRoot);
    if (json) {
        process.stdout.write(JSON.stringify({ projectRoot: path.resolve(projectRoot), findings }, null, 2) + '\n');
        return;
    }
    const labels = { protocol: '协议升级必须核查', 'strict-csp': '严格 CSP 时迁移', audit: '人工核查' };
    console.log('Core RazorForms 升级扫描：只读提示，不修改源码；不适用于原生 MVC / RazorPages 页面。');
    for (const finding of findings) {
        console.log(`${finding.file}:${finding.line} [${labels[finding.category]}] ${finding.message}`);
    }
    console.log(`共 ${findings.length} 条提示。文本扫描不能证明任意表达式、包装方法或条件分支安全，需结合实际运行核查。`);
}

if (process.argv[1] && path.resolve(process.argv[1]) === fileURLToPath(import.meta.url)) {
    try {
        main(process.argv.slice(2));
    } catch (error) {
        console.error(error.message);
        process.exitCode = 1;
    }
}
