FineUI 自定义主题说明
=====================

每个主题由以下文件组成：

  theme.config       颜色配置（必须）
  theme-extra.css    主题特有样式（可选，如背景图片）
  theme.css          自动生成，不要手动修改


生成主题
---------------------
  双击 generate-theme.bat                 生成所有主题
  node generate-theme.mjs                 同上
  node generate-theme.mjs my_theme        生成指定主题


创建新主题
---------------------
1. 新建目录，如 my_theme/

2. 复制一个已有的 theme.config，修改颜色值
   文件格式为 key = value，# 开头的行为注释

3. 可选：创建 theme-extra.css 添加自定义样式（如背景图片）

4. 运行 node generate-theme.mjs my_theme

5. 完成！


theme.config 配置项说明
---------------------
# 核心颜色（必填，6 组状态 × 3 + 圆角）
content-border-color = #ddd           内容区域边框
content-background-color = #fff       内容区域背景
content-text-color = #444             内容区域文字
header-border-color = #ddd            标题栏边框
header-background-color = #e6e6e6     标题栏背景
header-text-color = #444              标题栏文字
default-border-color = #ddd           默认状态边框
default-background-color = #fff       默认状态背景
default-text-color = #444             默认状态文字
hover-border-color = #40a9ff          悬停状态边框
hover-background-color = #f6f6f6      悬停状态背景
hover-text-color = #40a9ff            悬停状态文字
active-border-color = #1890ff         选中状态边框
active-background-color = #e6f7ff     选中状态背景
active-text-color = #1890ff           选中状态文字
error-border-color = #ffa8a8          错误状态边框
error-background-color = #fff8f8      错误状态背景
error-text-color = #ff6c6c            错误状态文字
border-radius = 6px                   圆角大小

# 标志位（可选）
is-dark-background = false            深色主题设为 true（默认 false）
is-dark-active-color = true           选中行底色深、文字色切换为高对比色时设为 true；
                                      选中行底色淡、文字色与正常状态基本同色时设为 false（默认 true）

# 派生变量（可选，不填则自动从 active 颜色取值）
primary-background-color = #1890ff    主色调背景
primary-text-color = #fff             主色调文字
tabstrip-inkbar-color = #1890ff       选项卡下划线颜色
focus-shadow-alpha = .15              焦点阴影透明度（深色主题建议 .4）

# 标题栏 hover/active（可选，深色标题栏主题需要单独定义）
header-hover-border-color = ...
header-hover-background-color = ...
header-hover-text-color = ...
header-active-border-color = ...
header-active-background-color = ...
header-active-text-color = ...

# 首页标题栏（可选，不填则使用 header 的默认值）
index-header-border-color = ...
index-header-background-color = ...
index-header-text-color = ...
index-hover-border-color = ...
index-hover-background-color = ...
index-hover-text-color = ...
index-active-border-color = ...
index-active-background-color = ...
index-active-text-color = ...
