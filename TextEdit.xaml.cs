namespace TextEditLib
{
    using AvalonEditB;
    using AvalonEditB.Folding;
    using AvalonEditB.Highlighting;
    using AvalonEditB.Indentation;
    using AvalonEditB.Indentation.CSharp;
    using AvalonEditB.Rendering;
    using AvalonEditB.Search;
    using System.Windows;
    using System.Windows.Media;
    using TextEditLib.Extensions;
    using TextEditLib.Foldings;

    /// <summary>
    /// Implements an AvalonEdit control textedit control with extensions.
    /// </summary>
    public class TextEdit : TextEditor
    {
        #region fields
        #region EditorCurrentLine Highlighting Colors
        private static readonly DependencyProperty EditorCurrentLineBackgroundProperty =
            DependencyProperty.Register("EditorCurrentLineBackground",
                                         typeof(Brush),
                                         typeof(TextEdit),
                                         new UIPropertyMetadata(new SolidColorBrush(Colors.Transparent)));

        public static readonly DependencyProperty EditorCurrentLineBorderProperty =
            DependencyProperty.Register("EditorCurrentLineBorder", typeof(Brush),
                typeof(TextEdit), new PropertyMetadata(new SolidColorBrush(
                    Color.FromArgb(0x60, SystemColors.HighlightBrush.Color.R,
                                         SystemColors.HighlightBrush.Color.G,
                                         SystemColors.HighlightBrush.Color.B))));

        public static readonly DependencyProperty EditorCurrentLineBorderThicknessProperty =
            DependencyProperty.Register("EditorCurrentLineBorderThickness", typeof(double),
                typeof(TextEdit), new PropertyMetadata(2.0d));

        // 定义行号依赖属性
        public static readonly DependencyProperty CurrentLineProperty =
            DependencyProperty.Register(
                "CurrentLine",
                typeof(int),
                typeof(TextEdit),
                new PropertyMetadata(1));

        // 定义列号依赖属性
        public static readonly DependencyProperty CurrentColumnProperty =
            DependencyProperty.Register(
                "CurrentColumn",
                typeof(int),
                typeof(TextEdit),
                new PropertyMetadata(1));

        // 定义选中信息依赖属性
        public static readonly DependencyProperty SelectionInfoProperty =
            DependencyProperty.Register(
                "SelectionInfo",
                typeof(string),
                typeof(TextEdit),
                new PropertyMetadata(string.Empty));
        public int CurrentLine
        {
            get { return (int)GetValue(CurrentLineProperty); }
            set { SetValue(CurrentLineProperty, value); }
        }

        public int CurrentColumn
        {
            get { return (int)GetValue(CurrentColumnProperty); }
            set { SetValue(CurrentColumnProperty, value); }
        }

        public string SelectionInfo
        {
            get { return (string)GetValue(SelectionInfoProperty); }
            set { SetValue(SelectionInfoProperty, value); }
        }
        #endregion EditorCurrentLine Highlighting Colors

        /// <summary>
        /// SyntaxHighlighting Dependency Property
        /// </summary>
        public static readonly new DependencyProperty SyntaxHighlightingProperty =
            TextEditor.SyntaxHighlightingProperty.AddOwner(typeof(TextEdit), new FrameworkPropertyMetadata(OnSyntaxHighlightingChanged));

        /// <summary>
        /// Document property.
        /// </summary>
        public static readonly new DependencyProperty DocumentProperty
            = TextView.DocumentProperty.AddOwner(
                typeof(TextEdit), new FrameworkPropertyMetadata(OnDocumentChanged));

        FoldingManager mFoldingManager = null;
        object mFoldingStrategy = null;
        #endregion fields

        #region ctors
        /// <summary>
        /// Static class constructor
        /// </summary>
        static TextEdit()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TextEdit),
                new FrameworkPropertyMetadata(typeof(TextEdit)));
        }

        /// <summary>
        /// Class constructor
        /// </summary>
        public TextEdit()
        {
            this.Loaded += TextEdit_Loaded;
        }
        #endregion ctors

        #region properties
        #region EditorCurrentLine Highlighting Colors
        /// <summary>
        /// Gets/sets the background color of the current editor line.
        /// </summary>
        public Brush EditorCurrentLineBackground
        {
            get { return (Brush)GetValue(EditorCurrentLineBackgroundProperty); }
            set { SetValue(EditorCurrentLineBackgroundProperty, value); }
        }

        /// <summary>
        /// Gets/sets the border color of the current editor line.
        /// </summary>
        public Brush EditorCurrentLineBorder
        {
            get { return (Brush)GetValue(EditorCurrentLineBorderProperty); }
            set { SetValue(EditorCurrentLineBorderProperty, value); }
        }

        /// <summary>
        /// Gets/sets the the thickness of the border of the current editor line.
        /// </summary>
        public double EditorCurrentLineBorderThickness
        {
            get { return (double)GetValue(EditorCurrentLineBorderThicknessProperty); }
            set { SetValue(EditorCurrentLineBorderThicknessProperty, value); }
        }
        #endregion EditorCurrentLine Highlighting Colors
        #endregion properties

        #region methods
        private void TextEdit_Loaded(object sender, RoutedEventArgs e)
        {
            SearchPanel.Install(this);
            AdjustCurrentLineBackground();
            // 订阅光标位置改变事件
            this.TextArea.Caret.PositionChanged += TextEdit_CaretPositionChanged;
            this.TextArea.SelectionChanged += TextEdit_SelectionChanged;
        }
        private void TextEdit_CaretPositionChanged(object sender, System.EventArgs e)
        {
            UpdateLineColumnInfo();
        }

        private void TextEdit_SelectionChanged(object sender, System.EventArgs e)
        {
            UpdateLineColumnInfo();
        }
        private void UpdateLineColumnInfo()
        {
            // 在UI线程上更新属性
            Dispatcher.Invoke(() =>
            {
                if (Document == null)
                {
                    CurrentLine = 0;
                    CurrentColumn = 0;
                    return;
                }

                if (this.TextArea.Selection.IsEmpty)
                {
                    // 无选中文本，显示光标位置
                    CurrentLine = Document.GetLineByOffset(CaretOffset).LineNumber;
                    CurrentColumn = CaretOffset - Document.GetLineByOffset(CaretOffset).Offset + 1;
                    SelectionInfo = string.Empty;
                }
                else
                {
                    // 有选中文本，计算选中范围
                    int selectionStartLine = Document.GetLineByOffset(TextArea.Selection.SurroundingSegment.Offset).LineNumber;
                    int selectionEndLine = Document.GetLineByOffset(TextArea.Selection.SurroundingSegment.Offset + TextArea.Selection.SurroundingSegment.Length).LineNumber;
                    int selectedLines = selectionEndLine - selectionStartLine + 1;
                    int selectedChars = TextArea.Selection.SurroundingSegment.Length;
                    // 设置选中信息
                    SelectionInfo = $"{selectedLines}行, {selectedChars}个字符被选中";

                    // 可选：选中文本时隐藏行列信息
                    CurrentLine = 0;
                    CurrentColumn = 0;
                }
            });
        }
        /// <summary>
        /// Reset the <seealso cref="SolidColorBrush"/> to be used for highlighting the current editor line.
        /// </summary>
        private void AdjustCurrentLineBackground()
        {
            HighlightCurrentLineBackgroundRenderer oldRenderer = null;

            // Make sure there is only one of this type of background renderer
            // Otherwise, we might keep adding and WPF keeps drawing them on top of each other
            foreach (var item in this.TextArea.TextView.BackgroundRenderers)
            {
                if (item != null)
                {
                    if (item is HighlightCurrentLineBackgroundRenderer)
                    {
                        oldRenderer = item as HighlightCurrentLineBackgroundRenderer;
                    }
                }
            }

            if (oldRenderer != null)
                this.TextArea.TextView.BackgroundRenderers.Remove(oldRenderer);

            this.TextArea.TextView.BackgroundRenderers.Add(new HighlightCurrentLineBackgroundRenderer(this));
        }
        #endregion methods
        private static void OnSyntaxHighlightingChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textEdit = d as TextEdit;
            if (textEdit != null)
                textEdit.OnChangedFoldingInstance(e);
        }
        private static void OnDocumentChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var textEdit = d as TextEdit;
            if (textEdit != null)
                textEdit.OnDocumentChanged(e);
        }

        private void OnDocumentChanged(DependencyPropertyChangedEventArgs e)
        {
            // Clean up and re-install foldings to avoid exception 'Invalid Document' being thrown by StartGeneration
            OnChangedFoldingInstance(e);
        }

        /// <summary>
        /// Method is invoked when the Document or SyntaxHighlightingDefinition dependency property is changed.
        /// This change should always lead to removing and re-installing the correct folding manager and strategy.
        /// </summary>
        /// <param name="e"></param>
        private void OnChangedFoldingInstance(DependencyPropertyChangedEventArgs e)
        {
            try
            {
                // Clean up last installation of folding manager and strategy
                if (mFoldingManager != null)
                {
                    FoldingManager.Uninstall(mFoldingManager);
                    mFoldingManager = null;
                }

                this.mFoldingStrategy = null;
            }
            catch
            {
            }

            if (e == null)
                return;

            var syntaxHighlighting = e.NewValue as IHighlightingDefinition;
            if (syntaxHighlighting == null)
                return;

            switch (syntaxHighlighting.Name)
            {
                case "XML":
                    mFoldingStrategy = new XmlFoldingStrategy();
                    this.TextArea.IndentationStrategy = new DefaultIndentationStrategy();
                    break;

                case "C#":
                case "C++":
                case "GOM":
                    mFoldingStrategy = new legendFoldingStrategy();
                    this.TextArea.IndentationStrategy = new DefaultIndentationStrategy();
                    break;
                case "PHP":
                case "Java":
                    this.TextArea.IndentationStrategy = new CSharpIndentationStrategy(this.Options);
                    mFoldingStrategy = new BraceFoldingStrategy();
                    break;
                case "INI": // 添加对 INI 文件的支持
                    mFoldingStrategy = new IniFoldingStrategy();
                    this.TextArea.IndentationStrategy = new DefaultIndentationStrategy();
                    break;
                default:
                    this.TextArea.IndentationStrategy = new DefaultIndentationStrategy();
                    mFoldingStrategy = null;
                    break;
            }

            if (mFoldingStrategy != null)
            {
                if (mFoldingManager == null)
                {
                    mFoldingManager = FoldingManager.Install(this.TextArea);
                }

                UpdateFoldings();
            }
            else
            {
                if (mFoldingManager != null)
                {
                    FoldingManager.Uninstall(mFoldingManager);
                    mFoldingManager = null;
                }
            }
        }

        private void UpdateFoldings()
        {
            if (mFoldingStrategy is BraceFoldingStrategy)
            {
                ((BraceFoldingStrategy)mFoldingStrategy).UpdateFoldings(mFoldingManager, this.Document);
            }

            if (mFoldingStrategy is XmlFoldingStrategy)
            {
                ((XmlFoldingStrategy)mFoldingStrategy).UpdateFoldings(mFoldingManager, this.Document);
            }
            if (mFoldingStrategy is legendFoldingStrategy)
                ((legendFoldingStrategy)mFoldingStrategy).UpdateFoldings(mFoldingManager, this.Document);
            if (mFoldingStrategy is IniFoldingStrategy)
                ((IniFoldingStrategy)mFoldingStrategy).UpdateFoldings(mFoldingManager, this.Document);
        }
    }
}
