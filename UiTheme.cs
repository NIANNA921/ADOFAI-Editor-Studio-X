using UnityEngine;

namespace ADOFAIEditorStudio.Ui
{
    /// <summary>
    /// 一套主题的全部语义取值。控件只读 <see cref="Current"/>，绝不直接引用 UiPrimitives。
    ///
    /// 加一套新主题 = 再写一个 UiTheme 实例 + 调一次 <see cref="Select"/>，
    /// **控件代码一行都不用改**。这是整个 token 分层存在的唯一理由。
    /// </summary>
    internal sealed class UiTheme
    {
        internal string Id;
        internal string DisplayName;

        // ── 表面 ──────────────────────────────────────────────────────────
        // 从最底到最上是 page → panel → raised → overlay，四档足够拉开层次；
        // well 是唯一往下沉的一档（输入框凹槽），比 page 还深。
        internal Color SurfacePage;
        internal Color SurfacePanel;
        internal Color SurfaceRaised;
        /// <summary>悬停态的凸起面。比 SurfaceRaised 再亮一档。</summary>
        internal Color SurfaceRaisedHover;
        internal Color SurfaceOverlay;
        internal Color SurfaceWell;
        /// <summary>比 Raised 更高一档，给凸起卡片内部的内容用（缩略图、色块）。</summary>
        internal Color SurfaceThumb;
        /// <summary>
        /// 面板标题条的实心底衬。比面板亮一档，四条横带才立得起来。
        /// </summary>
        internal Color SurfaceBand;
        /// <summary>
        /// 顶栏带（标题 / 菜单 / 标签条那三行）的底色。
        ///
        /// 全局背景已改纯黑，顶栏再纯黑就和内容糊成一片，所以这里单独留一档
        /// 极浅的灰（原来的 #0B0B0B）。**无边框、无圆角**，只是一块满宽矩形。
        /// </summary>
        internal Color SurfaceTopBand;



        // ── 文字 ──────────────────────────────────────────────────────────
        // 三级足够：正文 / 次要 / 弱化。等级再多就分不出主次了。
        internal Color TextPrimary;
        internal Color TextSecondary;
        internal Color TextMuted;
        internal Color TextDisabled;
        /// <summary>
        /// 强调文字：把关键词从正文里拎出来（快捷键的键名、条目的功能名、小节标题）。
        ///
        /// 它**比主色暗一档**。主色是给"能点的东西"用的，文字上再用一次整屏就吵了 ——
        /// 而且强调色是撒在正文里的，用量比按钮大得多。取的是恰好还过得了对比度的
        /// 那一档（见 Slate 里的取值说明），再暗就不该用来印正文了。
        /// </summary>
        internal Color TextEmphasis;
        /// <summary>压在亮色填充上的文字（比如主按钮）。</summary>
        internal Color TextOnAccent;

        // ── 边框 ──────────────────────────────────────────────────────────
        /// <summary>发丝描边。装饰用，不承担「必须看得见」的职责，对比度可以很低。</summary>
        internal Color BorderHairline;
        /// <summary>需要被辨识的控件边框（复选框勾选态、输入框聚焦态）。对比度需 ≥ 3:1。</summary>
        internal Color BorderStrong;
        /// <summary>聚焦环。</summary>
        internal Color BorderFocus;

        // ── 操作 ──────────────────────────────────────────────────────────
        internal Color ActionPrimary;
        internal Color ActionPrimaryHover;
        internal Color ActionPrimaryPressed;

        /// <summary>
        /// 第二强调色。这一套里它和主色**是同一个值**。
        ///
        /// 存在的理由只是让"标题/读数"和"按钮/选中"这两类位置各有一个名字，
        /// 将来想在某一类上分开调色时不必再去改控件。雾蓝这一版刻意只留
        /// 一个彩色——全屏只有一种蓝，界面才不会花。
        /// </summary>
        internal Color AccentSecondary;

        /// <summary>Light/dark aware overlays for hover, hairlines and carets.</summary>
        internal Color HoverFill;
        internal Color BorderOverlay;
        internal Color Caret;
        internal Color RowStripe;

        // ── 选中 ──────────────────────────────────────────────────────────
        internal Color SelectedFill;
        internal Color SelectedHoverFill;

        // ── 反馈 ──────────────────────────────────────────────────────────
        internal Color FeedbackDanger;
        internal Color FeedbackWarning;
        internal Color FeedbackSuccess;

        // ── 品牌渐变（2026-11-06）──────────────────────────────────────────
        /// <summary>
        /// 顶栏 logo（「ADOFAI EDITOR STUDIO」）的左→右三色渐变。
        /// 用户给的：E60013 → BF442E → 78141B，朱红到暗红。
        /// </summary>
        internal Color[] BrandGradient;

        /// <summary>
        /// 面板标题（预览 / 参数 / 媒体 / 时间轴 …）的颜色。
        /// 用户指定纯色 FB1F07（2026-11-06）。原来是双色渐变，改回单色。
        /// </summary>
        internal Color HeadingColor;

        // ── 阴影 ──────────────────────────────────────────────────────────
        internal Color Shadow;

        /// <summary>完全透明。用于「这一层不再自己画底色」的表面。</summary>
        private static readonly Color Transparent = new Color(0f, 0f, 0f, 0f);

        /// <summary>
        /// NEXT —— **AESX 默认主题**。
        ///
        /// 取自 AESX 的 Studio 配色：分层深灰面板、实心输入槽、蓝色选择/聚焦，
        /// 保留危险 / 警告 / 成功反馈色。设置里与「原生」（AES 0.39.5 的
        /// 纯灰阶 Mono）并列，可随时切回原版观感。
        /// </summary>
        internal static readonly UiTheme Studio = new UiTheme
        {
            Id = "next",
            DisplayName = "NEXT",

            SurfacePage = UiPrimitives.Hex("181818"),
            SurfacePanel = UiPrimitives.Hex("232323"),
            SurfaceRaised = UiPrimitives.Hex("303030"),
            SurfaceRaisedHover = UiPrimitives.Hex("3B3B3B"),
            SurfaceOverlay = UiPrimitives.Hex("2C2C2C"),
            SurfaceWell = UiPrimitives.Hex("191919"),
            SurfaceThumb = UiPrimitives.Hex("292929"),
            SurfaceBand = UiPrimitives.Hex("2B2B2B"),
            SurfaceTopBand = UiPrimitives.Hex("202020"),

            TextPrimary = UiPrimitives.Hex("E0E0E0"),
            TextSecondary = UiPrimitives.Hex("B8B8B8"),
            TextMuted = UiPrimitives.Hex("949494"),
            TextDisabled = UiPrimitives.Hex("6B6B6B"),
            TextEmphasis = UiPrimitives.Hex("6AA9F4"),
            TextOnAccent = UiPrimitives.Hex("101820"),

            BorderHairline = new Color(1f, 1f, 1f, 0.10f),
            BorderStrong = UiPrimitives.Hex("858585"),
            BorderFocus = UiPrimitives.Hex("6AA9F4"),

            ActionPrimary = UiPrimitives.Hex("4C9AFF"),
            ActionPrimaryHover = UiPrimitives.Hex("75B2FF"),
            ActionPrimaryPressed = UiPrimitives.Hex("3983DE"),

            AccentSecondary = UiPrimitives.Hex("D4D4D4"),

            HoverFill = new Color(1f, 1f, 1f, 0.08f),
            BorderOverlay = new Color(1f, 1f, 1f, 0.10f),
            Caret = UiPrimitives.Hex("F5F5F5"),
            RowStripe = new Color(1f, 1f, 1f, 0.030f),

            SelectedFill = new Color(0.298f, 0.604f, 1f, 0.22f),
            SelectedHoverFill = new Color(0.298f, 0.604f, 1f, 0.32f),

            FeedbackDanger = UiPrimitives.Hex("F08080"),
            FeedbackWarning = UiPrimitives.Hex("E5B967"),
            FeedbackSuccess = UiPrimitives.Hex("82C9A0"),

            // logo 保持灰阶，不再抢蓝色强调色。
            BrandGradient = new[]
            {
                UiPrimitives.Hex("E0E0E0"),
                UiPrimitives.Hex("B8B8B8"),
                UiPrimitives.Hex("949494")
            },
            HeadingColor = UiPrimitives.Hex("D4D4D4"),

            Shadow = Transparent
        };

        /// <summary>
        /// 原生 —— AES 0.39.5 原本的 Mono 纯灰阶主题。
        /// 与 NEXT 并列放进主题下拉；默认仍是 NEXT。
        /// </summary>
        /// <summary>
        /// Paper — light theme: white paper surfaces, lavender accent.
        /// </summary>
        internal static readonly UiTheme Paper = new UiTheme
        {
            Id = "paper",
            DisplayName = "纸白淡紫",

            SurfacePage = UiPrimitives.Hex("F4F1FA"),
            SurfacePanel = UiPrimitives.Hex("FFFFFF"),
            SurfaceRaised = UiPrimitives.Hex("F3EEFC"),
            SurfaceRaisedHover = UiPrimitives.Hex("E9E1FA"),
            SurfaceOverlay = UiPrimitives.Hex("FFFFFF"),
            SurfaceWell = UiPrimitives.Hex("F7F3FC"),
            SurfaceThumb = UiPrimitives.Hex("F0EAF9"),
            SurfaceBand = UiPrimitives.Hex("F8F5FD"),
            SurfaceTopBand = UiPrimitives.Hex("FFFFFF"),

            TextPrimary = UiPrimitives.Hex("2B2537"),
            TextSecondary = UiPrimitives.Hex("5F566F"),
            TextMuted = UiPrimitives.Hex("81798F"),
            TextDisabled = UiPrimitives.Hex("B8B0C4"),
            TextEmphasis = UiPrimitives.Hex("6A57E8"),
            TextOnAccent = UiPrimitives.Hex("FFFFFF"),

            BorderHairline = UiPrimitives.Hex("E7E1F0"),
            BorderStrong = UiPrimitives.Hex("C9BFE0"),
            BorderFocus = UiPrimitives.Hex("7564EE"),

            ActionPrimary = UiPrimitives.Hex("7564EE"),
            ActionPrimaryHover = UiPrimitives.Hex("8B7CF6"),
            ActionPrimaryPressed = UiPrimitives.Hex("5F4BD6"),

            AccentSecondary = UiPrimitives.Hex("6A57E8"),

            HoverFill = new Color(0.17f, 0.13f, 0.25f, 0.035f),
            BorderOverlay = new Color(0.17f, 0.13f, 0.25f, 0.09f),
            Caret = UiPrimitives.Hex("2B2537"),
            RowStripe = new Color(0.17f, 0.13f, 0.25f, 0.025f),

            SelectedFill = new Color(0.459f, 0.392f, 0.933f, 0.16f),
            SelectedHoverFill = new Color(0.459f, 0.392f, 0.933f, 0.26f),

            FeedbackDanger = UiPrimitives.Hex("D9576B"),
            FeedbackWarning = UiPrimitives.Hex("C28B2C"),
            FeedbackSuccess = UiPrimitives.Hex("3E9B76"),

            BrandGradient = new[]
            {
                UiPrimitives.Hex("B9A7FF"),
                UiPrimitives.Hex("8B7CF6"),
                UiPrimitives.Hex("6A57E8")
            },
            HeadingColor = UiPrimitives.Hex("4C3E68"),

            Shadow = new Color(0.17f, 0.13f, 0.25f, 0.10f)
        };

        internal static readonly UiTheme Native = new UiTheme
        {
            Id = "native",
            DisplayName = "原生",

            SurfacePage = UiPrimitives.Hex("111111"),
            SurfacePanel = UiPrimitives.Hex("151515"),
            SurfaceRaised = Transparent,
            SurfaceRaisedHover = new Color(1f, 1f, 1f, 0.08f),
            SurfaceOverlay = UiPrimitives.Hex("131313"),
            SurfaceWell = Transparent,
            SurfaceThumb = Transparent,
            SurfaceBand = Transparent,
            SurfaceTopBand = UiPrimitives.Hex("151515"),

            TextPrimary = UiPrimitives.Hex("F2F2F2"),
            TextSecondary = UiPrimitives.Hex("B8B8B8"),
            TextMuted = UiPrimitives.Hex("8A8A8A"),
            TextDisabled = UiPrimitives.Hex("5C5C5C"),
            TextEmphasis = UiPrimitives.Hex("FFFFFF"),
            TextOnAccent = UiPrimitives.Hex("151515"),

            BorderHairline = new Color(1f, 1f, 1f, 0.08f),
            BorderStrong = UiPrimitives.Hex("8A8A8A"),
            BorderFocus = UiPrimitives.Hex("FFFFFF"),

            ActionPrimary = UiPrimitives.Hex("E8E8E8"),
            ActionPrimaryHover = UiPrimitives.Hex("FFFFFF"),
            ActionPrimaryPressed = UiPrimitives.Hex("C4C4C4"),

            AccentSecondary = UiPrimitives.Hex("E8E8E8"),

            HoverFill = new Color(1f, 1f, 1f, 0.08f),
            BorderOverlay = new Color(1f, 1f, 1f, 0.08f),
            Caret = UiPrimitives.Hex("F2F2F2"),
            RowStripe = new Color(1f, 1f, 1f, 0.030f),

            SelectedFill = new Color(1f, 1f, 1f, 0.14f),
            SelectedHoverFill = new Color(1f, 1f, 1f, 0.22f),

            FeedbackDanger = UiPrimitives.Hex("E8E8E8"),
            FeedbackWarning = UiPrimitives.Hex("C4C4C4"),
            FeedbackSuccess = UiPrimitives.Hex("FFFFFF"),

            BrandGradient = new[]
            {
                UiPrimitives.Hex("F2F2F2"),
                UiPrimitives.Hex("C4C4C4"),
                UiPrimitives.Hex("8A8A8A")
            },
            HeadingColor = UiPrimitives.Hex("F2F2F2"),

            Shadow = Transparent
        };

        /// <summary>按 Id 找主题；找不到返回 null（调用方负责回落到默认）。</summary>
        internal static UiTheme ById(string id)
        {
            if (string.Equals(id, Paper.Id, System.StringComparison.OrdinalIgnoreCase))
            {
                return Paper;
            }
            if (string.Equals(id, Studio.Id, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(id, "studio", System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(id, "slate", System.StringComparison.OrdinalIgnoreCase))
            {
                return Studio;
            }
            if (string.Equals(id, Native.Id, System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(id, "mono", System.StringComparison.OrdinalIgnoreCase)
                || string.Equals(id, "ember", System.StringComparison.OrdinalIgnoreCase))
            {
                return Native;
            }
            return null;
        }

        private static UiTheme current;

        /// <summary>当前主题。**默认 NEXT**（AESX 主题）。</summary>
        internal static UiTheme Current
        {
            get { return current ?? (current = Studio); }
        }

        /// <summary>
        /// 切换主题。**调用方必须重建整个工作台** —— 颜色是在建控件那一刻
        /// 读进去的，改 `Current` 不会让已经建好的控件变色。
        /// 见 `EditorStudioOverlay.RebuildWorkbench()`。
        /// </summary>
        internal static void Select(UiTheme theme)
        {
            if (theme != null)
            {
                current = theme;
            }
        }
    }
}
