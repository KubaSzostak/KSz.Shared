using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace GeoSoft.Controls
{
    public class ImageButton : Button
    {

        private Image image = new Image();
        private TextBlock textBlock = new TextBlock();

        private TextBlock toolTipHeaderBlock = new TextBlock();
        private TextBlock toolTipDescriptionBlock = new TextBlock();
        private ToolTip toolTip = new ToolTip();

        public ImageButton()
        {
            this.Padding = new Thickness(4, 1, 4, 1);

            var sp = new StackPanel();
            sp.Orientation = Orientation.Horizontal;
            sp.Children.Add(image);
            sp.Children.Add(textBlock);
            image.Stretch = Stretch.None;
            //image.VerticalAlignment = VerticalAlignment.Center;
            //textBlock.VerticalAlignment = VerticalAlignment.Center;
            textBlock.Margin = new Thickness(3, 0, 0, 0);
            Content = sp;

            toolTipHeaderBlock.FontWeight = FontWeights.Bold;
            toolTipDescriptionBlock.TextWrapping = TextWrapping.Wrap;
            toolTipDescriptionBlock.Margin = new Thickness(0, 3, 0, 0);
            var ttSp = new StackPanel();
            ttSp.Margin = new Thickness(4);
            ttSp.Children.Add(toolTipHeaderBlock);
            ttSp.Children.Add(toolTipDescriptionBlock);
            toolTip.Content = ttSp;
            toolTip.MaxWidth = 400;

            //this.HorizontalContentAlignment = System.Windows.HorizontalAlignment.Center;
            //this.VerticalContentAlignment = System.Windows.VerticalAlignment.Center;
        }


        public string Label
        {
            get
            {
                return textBlock.Text;
            }
            set
            {
                textBlock.Text = value;
            }
        }

        public ImageSource Image
        {
            get
            {
                return image.Source;
            }
            set
            {
                image.Source = value;
            }
        }

        public string ToolTipHeader
        {
            get
            {
                return toolTipHeaderBlock.Text;
            }
            set
            {
                toolTipHeaderBlock.Text = value;
            }
        }

        public string ToolTipDescription
        {
            get
            {
                return toolTipDescriptionBlock.Text;
            }
            set
            {
                toolTipDescriptionBlock.Text = value;
                if (string.IsNullOrEmpty(value))
                    ToolTipService.SetToolTip(this, null);
                else
                    ToolTipService.SetToolTip(this, toolTip);

            }
        }

    }
}
