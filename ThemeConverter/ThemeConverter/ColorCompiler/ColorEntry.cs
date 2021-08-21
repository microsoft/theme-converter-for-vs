using System;
using System.ComponentModel;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Media;
using Color = System.Drawing.Color;

namespace ThemeConverter.ColorCompiler
{
    public class ColorEntry : DependencyObject
    {
        public ColorEntry(ColorName name)
        {
            Name = name;
        }

        private static void SetBackgroundBrush(ColorEntry entry)
        {
            SolidColorBrush brush = new SolidColorBrush(entry.Background);
            brush.Freeze();
            entry.BackgroundBrush = brush;
        }

        private static void SetForegroundBrush(ColorEntry entry)
        {
            SolidColorBrush brush = new SolidColorBrush(entry.Foreground);
            brush.Freeze();
            entry.ForegroundBrush = brush;
        }

        private static readonly DependencyPropertyKey NamePropertyKey = DependencyProperty.RegisterReadOnly("Name", typeof(ColorName), typeof(ColorEntry), new PropertyMetadata(null));
        public static readonly DependencyProperty NameProperty = NamePropertyKey.DependencyProperty;

        public ColorName Name
        {
            get
            {
                return (ColorName)GetValue(NameProperty);
            }
            private set
            {
                SetValue(NamePropertyKey, value);
            }
        }

        public static readonly DependencyProperty ThemeProperty = DependencyProperty.Register("Theme", typeof(ColorTheme), typeof(ColorEntry));

        public ColorTheme Theme
        {
            get
            {
                return (ColorTheme)GetValue(ThemeProperty);
            }
            set
            {
                SetValue(ThemeProperty, value);
            }
        }

        private static readonly DependencyPropertyKey IsEmptyPropertyKey = DependencyProperty.RegisterReadOnly("IsEmpty", typeof(bool), typeof(ColorEntry), new FrameworkPropertyMetadata(true));
        public static readonly DependencyProperty IsEmptyProperty = IsEmptyPropertyKey.DependencyProperty;

        public bool IsEmpty
        {
            get
            {
                return (bool)GetValue(IsEmptyProperty);
            }
            private set
            {
                SetValue(IsEmptyPropertyKey, value);
            }
        }

        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register("Background", typeof(Color), typeof(ColorEntry), new PropertyMetadata(Colors.Transparent, OnBackgroundChanged));

        public Color Background
        {
            get
            {
                return (Color)GetValue(BackgroundProperty);
            }
            set
            {
                SetValue(BackgroundProperty, value);
            }
        }

        private static readonly DependencyPropertyKey BackgroundBrushPropertyKey = DependencyProperty.RegisterReadOnly("BackgroundBrush", typeof(Brush), typeof(ColorEntry), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty BackgroundBrushProperty = BackgroundBrushPropertyKey.DependencyProperty;

        public Brush BackgroundBrush
        {
            get
            {
                return (Brush)GetValue(BackgroundBrushProperty);
            }
            private set
            {
                SetValue(BackgroundBrushPropertyKey, value);
            }
        }

        public static readonly DependencyProperty BackgroundSourceProperty = DependencyProperty.Register("BackgroundSource", typeof(uint), typeof(ColorEntry), new PropertyMetadata(0U, OnBackgroundSourceChanged));

        public uint BackgroundSource
        {
            get
            {
                return (uint)GetValue(BackgroundSourceProperty);
            }
            set
            {
                SetValue(BackgroundSourceProperty, value);
            }
        }

        public static readonly DependencyProperty BackgroundTypeProperty = DependencyProperty.Register("BackgroundType", typeof(__VSCOLORTYPE), typeof(ColorEntry), new PropertyMetadata(__VSCOLORTYPE.CT_INVALID, OnBackgroundSourceChanged));

        public __VSCOLORTYPE BackgroundType
        {
            get
            {
                return (__VSCOLORTYPE)GetValue(BackgroundTypeProperty);
            }
            set
            {
                SetValue(BackgroundTypeProperty, value);
            }
        }

        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register("Foreground", typeof(Color), typeof(ColorEntry), new PropertyMetadata(Colors.Transparent, OnForegroundChanged));

        public Color Foreground
        {
            get
            {
                return (Color)GetValue(ForegroundProperty);
            }
            set
            {
                SetValue(ForegroundProperty, value);
            }
        }

        private static readonly DependencyPropertyKey ForegroundBrushPropertyKey = DependencyProperty.RegisterReadOnly("ForegroundBrush", typeof(Brush), typeof(ColorEntry), new FrameworkPropertyMetadata(null));
        public static readonly DependencyProperty ForegroundBrushProperty = ForegroundBrushPropertyKey.DependencyProperty;

        public Brush ForegroundBrush
        {
            get
            {
                return (Brush)GetValue(ForegroundBrushProperty);
            }
            private set
            {
                SetValue(ForegroundBrushPropertyKey, value);
            }
        }

        public static readonly DependencyProperty ForegroundSourceProperty = DependencyProperty.Register("ForegroundSource", typeof(uint), typeof(ColorEntry), new PropertyMetadata(0U, OnForegroundSourceChanged));

        public uint ForegroundSource
        {
            get
            {
                return (uint)GetValue(ForegroundSourceProperty);
            }
            set
            {
                SetValue(ForegroundSourceProperty, value);
            }
        }

        public static readonly DependencyProperty ForegroundTypeProperty = DependencyProperty.Register("ForegroundType", typeof(__VSCOLORTYPE), typeof(ColorEntry), new PropertyMetadata(__VSCOLORTYPE.CT_INVALID, OnForegroundSourceChanged));

        public __VSCOLORTYPE ForegroundType
        {
            get
            {
                return (__VSCOLORTYPE)GetValue(ForegroundTypeProperty);
            }
            set
            {
                SetValue(ForegroundTypeProperty, value);
            }
        }

        static void RecordUndo(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            UndoManager manager = UndoManager.GlobalUndoManager;
            if (!manager.IsProcessingChange && manager.CurrentTransaction != null)
            {
                manager.Add(new ApplyPropertyUndoUnit(obj, e.Property, e.OldValue, e.NewValue));
            }
        }

        static void OnBackgroundChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            RecordUndo(obj, e);

            ColorEntry entry = (ColorEntry)obj;
            SetBackgroundBrush(entry);
        }

        static void OnBackgroundSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            RecordUndo(obj, e);

            ColorEntry entry = (ColorEntry)obj;
            entry.UpdateColor(entry.BackgroundType, entry.BackgroundSource, BackgroundProperty);
            entry.UpdateIsEmpty();
        }

        static void OnForegroundChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            RecordUndo(obj, e);

            ColorEntry entry = (ColorEntry)obj;
            SetForegroundBrush(entry);
        }

        static void OnForegroundSourceChanged(DependencyObject obj, DependencyPropertyChangedEventArgs e)
        {
            RecordUndo(obj, e);

            ColorEntry entry = (ColorEntry)obj;
            entry.UpdateColor(entry.ForegroundType, entry.ForegroundSource, ForegroundProperty);
            entry.UpdateIsEmpty();
        }

        void UpdateColor(__VSCOLORTYPE type, uint source, DependencyProperty targetProperty)
        {
            switch (type)
            {
                case __VSCOLORTYPE.CT_RAW:
                    {
                        BindingOperations.ClearBinding(this, targetProperty);
                        Color color = FromRgba(source);
                        SetValue(targetProperty, color);
                    }
                    break;
                case __VSCOLORTYPE.CT_SYSCOLOR:
                    {
                        SystemColor systemColor = (SystemColor)source;
                        BindingOperations.SetBinding(this, targetProperty, new Binding()
                        {
                            Source = SystemColorHelper.GetSystemColorReference(systemColor),
                            Path = new PropertyPath(SystemColorReference.ColorProperty)
                        });
                    }
                    break;
                default:
                    BindingOperations.ClearBinding(this, targetProperty);
                    SetValue(targetProperty, Colors.White);
                    break;
            }
        }

        void UpdateIsEmpty()
        {
            IsEmpty = BackgroundType == __VSCOLORTYPE.CT_INVALID && ForegroundType == __VSCOLORTYPE.CT_INVALID;
        }

        public static Color FromRgba(uint rgba)
        {
            byte alpha = (byte)(rgba >> 24);
            byte blue = (byte)(rgba >> 16);
            byte green = (byte)(rgba >> 8);
            byte red = (byte)rgba;
            return Color.FromArgb(alpha, red, green, blue);
        }
    }
}
