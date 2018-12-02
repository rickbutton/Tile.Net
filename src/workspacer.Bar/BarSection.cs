﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;

namespace workspacer.Bar
{
    public class BarSection
    {
        private FlowLayoutPanel _panel;
        private IBarWidget[] _widgets;
        private IMonitor _monitor;
        private IConfigContext _configContext;
        private int _fontSize;

        private Color _defaultFore;
        private Color _defaultBack;

        private bool _reverse;
        private bool _dirty;
        private IBarWidgetContext _context;

        private IDictionary<Label, Action> _clickedHandlers;

        public BarSection(bool reverse, FlowLayoutPanel panel, IBarWidget[] widgets, IMonitor monitor, IConfigContext context, 
            Color defaultFore, Color defaultBack, int fontSize)
        {
            _panel = panel;
            _widgets = widgets;
            _monitor = monitor;
            _configContext = context;
            _fontSize = fontSize;
            _dirty = true;
            _reverse = reverse;
            _defaultFore = defaultFore;
            _defaultBack = defaultBack;


            _clickedHandlers = new Dictionary<Label, Action>();

            _context = new BarWidgetContext(this, _monitor, _configContext);
            InitializeWidgets(widgets, _context);
        }

        public void Draw()
        {
            if (_dirty)
            {
                var widgets = _reverse ? _widgets.Reverse().ToArray() : _widgets;

                int partNumber = 0;
                for (var i = 0; i < widgets.Length; i++)
                {
                    var widget = widgets[i];
                    var parts = widget.GetParts();
                    for (var j = 0; j < parts.Length; j++)
                    {
                        var part = parts[j];

                        if (partNumber < _panel.Controls.Count)
                        {
                            Label label = (Label)_panel.Controls[partNumber];
                            SetLabel(label, part);
                        }
                        else
                        {
                            var label = AddLabel();
                            SetLabel(label, part);
                        }
                        partNumber++;
                    }
                }

                var toRemove = new List<Control>();
                if (partNumber < _panel.Controls.Count - 1)
                {
                    for (var i = partNumber; i < _panel.Controls.Count; i++)
                    {
                        toRemove.Add(_panel.Controls[i]);
                    }
                }
                toRemove.ForEach(c => _panel.Controls.Remove(c));
                _dirty = false;
            }
        }

        private void SetLabel(Label label, IBarWidgetPart part)
        {
            label.Text = part.Text;
            if (part.ForegroundColor != null)
            {
                label.ForeColor = ColorToColor(part.ForegroundColor);
            } else
            {
                label.ForeColor = ColorToColor(_defaultFore);
            }

            if (part.BackgroundColor != null && part.BackgroundColor != _defaultFore)
            {
                label.BackColor = ColorToColor(part.BackgroundColor);
            } else
            {
                label.BackColor = System.Drawing.Color.FromArgb(0, System.Drawing.Color.Black);
            }

            if (part.PartClicked != null)
            {
                _clickedHandlers[label] = part.PartClicked;
            } else
            {
                _clickedHandlers.Remove(label);
            }
        }

        public void MarkDirty()
        {
            _dirty = true;
        }

        private System.Drawing.Color ColorToColor(Color color)
        {
            return System.Drawing.Color.FromArgb(color.R, color.G, color.B);
        }

        private Font CreateFont(float size)
        {
            return new Font("Consolas", size, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
        }

        private Label AddLabel()
        {
            Label label = new Label();
            _panel.Controls.Add(label);

            label.AutoSize = true;
            label.Font = CreateFont(_fontSize);
            label.Margin = new Padding(0);
            label.Padding = new Padding(0);

            label.Click += (s, e) =>
            {
                if (_clickedHandlers.ContainsKey(label))
                {
                    _clickedHandlers[label]();
                }
            };

            return label;
        }

        private void InitializeWidgets(IEnumerable<IBarWidget> widgets, IBarWidgetContext context)
        {
            foreach (var w in widgets)
            {
                w.Initialize(context);
            }
        }
    }
}
