using Autodesk.AutoCAD.Ribbon;
using Autodesk.Windows;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AcadMicsTests
{
    public class MyRibbon
    {
        private readonly RibbonControl _ribbonControl;
        private const string TAB_ID = "MY_CUSTOM_RIBBON_TAB";

        public MyRibbon()
        {
            _ribbonControl = ComponentManager.Ribbon;
        }

        public void CreateMyTab()
        {
            if (_ribbonControl==null)
            {
                throw new InvalidOperationException(
                    "Ribbon menu is not available.");
            }

            if (_ribbonControl.FindTab(TAB_ID) != null) return;

            BuildTab();
        }

        private void BuildTab()
        {
            var tab=new RibbonTab();
            tab.Title = "My Custom Tab";
            tab.Id= TAB_ID;
            AddRibbonPanelToTab(tab);
            _ribbonControl.Tabs.Add(tab);
            _ribbonControl.ActiveTab = tab;
        }

        private void AddRibbonPanelToTab(RibbonTab tab)
        {
            var source = new RibbonPanelSource { Title = "Custom Commands", Id = "MY_CUSTOM_COMMANDS" };
            var panel = new RibbonPanel { Source = source };

            RibbonRowPanel rowPanel;

            rowPanel = CreateRibbonRowPanel(
                new []
                {
                    new RibbonCommandButton{Text="Xxxxxx1", ShowText=true },
                    new RibbonCommandButton{Text="Xxxxxx2", ShowText=true },
                    new RibbonCommandButton{Text="Xxxxxx3", ShowText=true },
                });
            source.Items.Add(rowPanel);

            source.Items.Add(new RibbonRowBreak());

            rowPanel = CreateRibbonRowPanel(
                new []
                {
                    new RibbonCommandButton{Text="Yyyyyy1", ShowText = true},
                    new RibbonCommandButton{Text="Yyyyyy2", ShowText=true },
                    new RibbonCommandButton{Text="Yyyyyy3", ShowText = true},
                });
            source.Items.Add(rowPanel);

            source.Items.Add(new RibbonRowBreak());

            rowPanel = CreateRibbonRowPanel(
                new[]
                {
                    new RibbonCommandButton{Text="Zzzzzz1", ShowText=true },
                    new RibbonCommandButton{Text="Zzzzzz2", ShowText=true },
                    new RibbonCommandButton{Text="Zzzzzz3", ShowText=true },
                });
            source.Items.Add(rowPanel);

            tab.Panels.Add(panel);
        }

        private RibbonRowPanel CreateRibbonRowPanel(IEnumerable<RibbonCommandButton> buttons)
        {
            var rowPanel=new RibbonRowPanel();
            foreach (var btn in buttons)
            {
                rowPanel.Items.Add(btn);
            }
            return rowPanel;
        }
    }
}
