using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Media;

namespace InfoPointUI.Controls;

public class VirtualizingWrapPanel : VirtualizingPanel, IScrollInfo
{
    private Size _childSize = new(220, 260);
    private int _itemsPerRow;
    private int _firstIndex;
    private List<int> _visibleIndices = new();

    private ItemsControl? _itemsControl;
    private IItemContainerGenerator? _generator;

    private Size _extent = new(0, 0);
    private Size _viewport = new(0, 0);
    private Point _offset;

    public VirtualizingWrapPanel()
    {
        CanHorizontallyScroll = false;
        CanVerticallyScroll = true;
    }

    protected override void OnInitialized(EventArgs e)
    {
        base.OnInitialized(e);
        ScrollOwner ??= FindScrollViewerAncestor(this);
    }

    protected override Size MeasureOverride(Size availableSize)
    {
        _itemsControl ??= ItemsControl.GetItemsOwner(this);
        _generator ??= ItemContainerGenerator;

        double safeWidth = double.IsInfinity(availableSize.Width) ? _childSize.Width * 3 : availableSize.Width;
        double safeHeight = double.IsInfinity(availableSize.Height) ? _childSize.Height * 2 : availableSize.Height;
        Size safeSize = new(safeWidth, safeHeight);

        if (_itemsControl == null || _generator == null)
            return safeSize;

        int itemCount = _itemsControl.Items.Count;
        _itemsPerRow = Math.Max(1, (int)(safeSize.Width / _childSize.Width));
        int visibleRows = Math.Max(1, (int)(safeSize.Height / _childSize.Height));

        int startRow = (int)(_offset.Y / _childSize.Height);
        _firstIndex = Math.Min(itemCount - 1, startRow * _itemsPerRow);
        int visibleItemCount = Math.Min(itemCount - _firstIndex, _itemsPerRow * (visibleRows + 1));

        _visibleIndices.Clear();

        var children = InternalChildren;
        var startPos = _generator.GeneratorPositionFromIndex(_firstIndex);
        int childIndex = (startPos.Offset == 0) ? startPos.Index : startPos.Index + 1;

        using (_generator.StartAt(startPos, GeneratorDirection.Forward, true))
        {
            for (int i = 0; i < visibleItemCount && _firstIndex + i < itemCount; i++)
            {
                var child = _generator.GenerateNext(out bool isNew) as UIElement;
                if (child == null) continue;

                if (isNew)
                {
                    if (childIndex >= children.Count)
                        AddInternalChild(child);
                    else
                        InsertInternalChild(childIndex, child);

                    _generator.PrepareItemContainer(child);
                }

                child.Measure(_childSize);
                childIndex++;
                _visibleIndices.Add(_firstIndex + i);
            }
        }

        int totalRows = (int)Math.Ceiling((double)itemCount / _itemsPerRow);
        _extent = new Size(safeSize.Width, totalRows * _childSize.Height);
        _viewport = safeSize;

        ScrollOwner ??= FindScrollViewerAncestor(this);
        ScrollOwner?.InvalidateScrollInfo();

        return safeSize;
    }

    protected override Size ArrangeOverride(Size finalSize)
    {
        for (int i = 0; i < InternalChildren.Count && i < _visibleIndices.Count; i++)
        {
            int itemIndex = _visibleIndices[i];
            int row = itemIndex / _itemsPerRow;
            int col = itemIndex % _itemsPerRow;

            double x = col * _childSize.Width;
            double y = (row * _childSize.Height) - _offset.Y;

            InternalChildren[i].Arrange(new Rect(new Point(x, y), _childSize));
        }

        ScrollOwner ??= FindScrollViewerAncestor(this);
        ScrollOwner?.InvalidateScrollInfo();

        Debug.WriteLine($"Extent={_extent}, Viewport={_viewport}, Offset={_offset}");


        return finalSize;
    }

    private ScrollViewer? FindScrollViewerAncestor(DependencyObject? current)
    {
        while (current != null)
        {
            if (current is ScrollViewer sv)
                return sv;

            current = VisualTreeHelper.GetParent(current);
        }
        return null;
    }

    #region IScrollInfo

    public bool CanHorizontallyScroll { get; set; }
    public bool CanVerticallyScroll { get; set; }
    public double ExtentWidth => _extent.Width;
    public double ExtentHeight => _extent.Height;
    public double ViewportWidth => _viewport.Width;
    public double ViewportHeight => _viewport.Height;
    public double HorizontalOffset => _offset.X;
    public double VerticalOffset => _offset.Y;
    public ScrollViewer? ScrollOwner { get; set; }

    public void LineDown() => SetVerticalOffset(VerticalOffset + 20);
    public void LineUp() => SetVerticalOffset(VerticalOffset - 20);
    public void MouseWheelDown() => SetVerticalOffset(VerticalOffset + 40);
    public void MouseWheelUp() => SetVerticalOffset(VerticalOffset - 40);
    public void PageDown() => SetVerticalOffset(VerticalOffset + ViewportHeight);
    public void PageUp() => SetVerticalOffset(VerticalOffset - ViewportHeight);

    public void LineLeft() { }
    public void LineRight() { }
    public void PageLeft() { }
    public void PageRight() { }
    public void MouseWheelLeft() { }
    public void MouseWheelRight() { }

    public void SetVerticalOffset(double offset)
    {
        if (offset < 0 || ViewportHeight >= ExtentHeight)
            offset = 0;
        else if (offset + ViewportHeight >= ExtentHeight)
            offset = ExtentHeight - ViewportHeight;

        _offset.Y = offset;
        InvalidateMeasure();
        ScrollOwner?.InvalidateScrollInfo();
    }

    public void SetHorizontalOffset(double offset)
    {
        _offset.X = offset;
        ScrollOwner?.InvalidateScrollInfo();
    }

    public Rect MakeVisible(Visual visual, Rect rectangle) => rectangle;

    #endregion
}
