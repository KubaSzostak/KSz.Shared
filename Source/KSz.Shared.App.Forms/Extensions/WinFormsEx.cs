using System.Collections.Generic;
using System.Linq.Expressions;

namespace System.Windows.Forms
{
    public static class WinFormsEx
    {

        public static bool ItemsIsFirstSelected(this ListView lv)
        {
            if (lv.Items.Count < 1)
                return false;
            return lv.Items[0].Selected;
        }

        public static bool ItemsIsLastSelected(this ListView lv)
        {
            if (lv.Items.Count < 1)
                return false;
            return lv.Items[lv.Items.Count - 1].Selected;
        }

        private static void SelectedItemMove(this ListView lv, int upDown)
        {
            if (lv.SelectedItems.Count < 1)
                return;

            var selItem = lv.SelectedItems[0];
            var newIndex = selItem.Index + upDown;

            if ((newIndex < 0) || (newIndex >= lv.Items.Count))
                return;

            lv.Items.RemoveAt(selItem.Index);
            lv.Items.Insert(newIndex, selItem);
            lv.EnsureVisible(newIndex);
        }

        public static void SelectedItemMoveUp(this ListView lv)
        {
            lv.SelectedItemMove(-1);
        }

        public static void SelectedItemMoveDown(this ListView lv)
        {
            lv.SelectedItemMove(+1);
        }

        public static ListViewItem SelectedItemDelete(this ListView lv)
        {
            ListViewItem delItem = null;
            int selIndex = 0;
            while (lv.SelectedItems.Count > 0)
            {
                delItem = lv.SelectedItems[0];
                selIndex = delItem.Index;
                lv.SelectedItems[0].Remove();
            }

            if ((lv.Items.Count < 1))
                return delItem;

            if (selIndex >= lv.Items.Count)
                selIndex = lv.Items.Count - 1;

            lv.Items[selIndex].Selected = true;

            return delItem;
        }

        public static void SelectItem(this ComboBox box, object value)
        {
            if (box.Items.Count < 1)
                return;

            var i = box.Items.IndexOf(value);
            if (i < 0)
                i = 0;
            box.SelectedIndex = i;
        }

        public static void SelectFirstItem(this ComboBox box)
        {
            if (box.Items.Count > 0)
                box.SelectedItem = 0;
        }

        public static void WriteBindings(this ComboBox control)
        {
            for (int i = 0; i < control.DataBindings.Count; i++)
            {
                control.DataBindings[i].WriteValue();
            }
        }
        public static void ReadBindings(this ComboBox control)
        {
            for (int i = 0; i < control.DataBindings.Count; i++)
            {
                control.DataBindings[i].ReadValue();
            }
        }

        public static void SetTooltip(this Control control, string tooltip)
        {
            var toolTip = new System.Windows.Forms.ToolTip();
            toolTip.SetToolTip(control, tooltip);
        }
    }

}
