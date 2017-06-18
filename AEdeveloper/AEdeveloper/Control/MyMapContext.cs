using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using Crownwood.DotNetMagic.Docking;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.SystemUI;
using AEdeveloper.MyBaseTool;
using ESRI.ArcGIS.Carto;

namespace AEdeveloper.Control
{
    public partial class MyMapContext : UserControl
    {
        public Mainform _mainFrm;
        DockingManager _dockmanager1 = null;
        Crownwood.DotNetMagic.Docking.Content _workspaceContent = null;
        public MapSlightSeeing _SlightWorkSpaceCtrl;
        public MyMapContext(Mainform parent)
        {
            InitializeComponent();
            InitGui();
            _mainFrm = parent;
        }
        /// <summary>
        /// 初始化程序界面
        /// </summary>
        private void InitGui()
        {
            _dockmanager1 = new DockingManager(this.ChildrenPanel1, Crownwood.DotNetMagic.Common.VisualStyle.Office2007Silver);        
            //this.tabControlCenter.Dock = DockStyle.Fill;
            _dockmanager1.InnerControl = this.axTOCControl1;
            _SlightWorkSpaceCtrl = new MapSlightSeeing(this);
            _SlightWorkSpaceCtrl.Dock = DockStyle.Fill;
            _workspaceContent = _dockmanager1.Contents.Add(this._SlightWorkSpaceCtrl);
            _dockmanager1.AddContentWithState(_workspaceContent, Crownwood.DotNetMagic.Docking.State.DockBottom);
        }

        //实现TOCControl右键菜单
        private ITOCControl2 m_tocControl = null;//TOCControl控件变量 
        public IToolbarMenu m_menuMap = null;   //TOCControl中Map菜单
        private IToolbarMenu m_menuLayer = null;//TOCControl中图层菜单       
        public IMapControl3 m_mapControl = null;//MAPControl控件变量
        public DataTable attributeTable;//AttributeTableFrm类成员变量

        private void MyMapContext_Load(object sender, EventArgs e)
        {
            //首先初始化右键菜单
            m_tocControl = this.axTOCControl1.Object as ITOCControl2;//获取TOCC引用
            m_menuMap = new ToolbarMenuClass();
            m_menuLayer = new ToolbarMenuClass();
            m_mapControl = (IMapControl3)_mainFrm.axMapControl1.Object; //取得MapControl的引用
            //添加自定义菜单项到TOCCOntrol的Map菜单中 
            //打开文档菜单
            m_menuMap.AddItem(new  ControlsOpenDocCommandClass(), -1, 0, false, esriCommandStyles.esriCommandStyleIconAndText);
            //添加数据菜单
            m_menuMap.AddItem(new ControlsAddDataCommandClass(), -1, 1, false, esriCommandStyles.esriCommandStyleIconAndText);
            //打开全部图层菜单
            m_menuMap.AddItem(new LayerVisibility(1), 1, 2, false, esriCommandStyles.esriCommandStyleIconAndText);
            //关闭全部图层菜单
            m_menuMap.AddItem(new LayerVisibility(2), 1, 3, false, esriCommandStyles.esriCommandStyleIconAndText);

            //以二级菜单的形式添加内置的“选择”菜单
            m_menuMap.AddSubMenu("esriControls.ControlsFeatureSelectionMenu", 4, true);
            //以二级菜单的形式添加内置的“地图浏览”菜单
            m_menuMap.AddSubMenu("esriControls.ControlsMapViewMenu", 5, true);

            //添加自定义菜单项到TOCCOntrol的图层菜单中
            m_menuLayer = new ToolbarMenuClass();
            //添加“移除图层”菜单项
            m_menuLayer.AddItem(new RemoveLayer(), -1, 0, false, esriCommandStyles.esriCommandStyleTextOnly);
            //添加“放大到整个图层”菜单项
            m_menuLayer.AddItem(new ZoomToLayer(), -1, 1, true, esriCommandStyles.esriCommandStyleTextOnly);
            m_menuLayer.AddItem(new Controls3DAnalystContourToolClass(), 1, 2, true, esriCommandStyles.esriCommandStyleTextOnly);
            //设置菜单的Hook
            m_menuLayer.SetHook(m_mapControl);
            m_menuMap.SetHook(m_mapControl);
        }

        private void axTOCControl1_OnMouseDown(object sender, ITOCControlEvents_OnMouseDownEvent e)
        {
            //如果不是右键按下直接返回
            if (e.button != 2) return;
            esriTOCControlItem item = esriTOCControlItem.esriTOCControlItemNone;
            IBasicMap map = null;
            ILayer layer = null;
            object other = null;
            object index = null;
            //判断所选菜单的类型
            m_tocControl.HitTest(e.x, e.y, ref item, ref map, ref layer, ref other, ref index);
            //确定选定的菜单类型，Map或是图层菜单
            if (item == esriTOCControlItem.esriTOCControlItemMap)
                m_tocControl.SelectItem(map, null);
            else
                m_tocControl.SelectItem(layer, null);
            //设置CustomProperty为layer (用于自定义的Layer命令)			
            m_mapControl.CustomProperty = layer;
            //弹出右键菜单
            if (item == esriTOCControlItem.esriTOCControlItemMap)
                m_menuMap.PopupMenu(e.x, e.y, m_tocControl.hWnd);
            if (item == esriTOCControlItem.esriTOCControlItemLayer)
            {
                //动态添加OpenAttributeTable菜单项
                m_menuLayer.AddItem(new OpenAttributeTable(layer), -1, 2, true, esriCommandStyles.esriCommandStyleTextOnly);
                m_menuLayer.PopupMenu(e.x, e.y, m_tocControl.hWnd);
                //移除OpenAttributeTable菜单项，以防止重复添加
                m_menuLayer.Remove(2);
            }
        }

        private void axTOCControl1_OnDoubleClick(object sender, ITOCControlEvents_OnDoubleClickEvent e)
        {
            object unk = null;
            object data = null;

            // 双击TOCControl控件时触发的事件
            esriTOCControlItem itemType = esriTOCControlItem.esriTOCControlItemNone;
            IBasicMap basicMap = null;
            ILayer layer = null;
            this.axTOCControl1.HitTest(e.x, e.y, ref itemType, ref basicMap, ref layer, ref unk, ref data);
            if (e.button == 1)
            {
                if (itemType == esriTOCControlItem.esriTOCControlItemLegendClass)
                {
                    //取得图例
                    ILegendClass pLegendClass = ((ILegendGroup)unk).get_Class((int)data);
                    //创建符号选择器SymbolSelector实例
                    SymbolSelectorFrm SymbolSelectorFrm = new SymbolSelectorFrm(pLegendClass, layer);
                    if (SymbolSelectorFrm.ShowDialog() == DialogResult.OK)
                    {
                        //局部更新主Map控件
                        m_mapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, null, null);
                        //设置新的符号
                        pLegendClass.Symbol = SymbolSelectorFrm.pSymbol;
                        //更新主Map控件和图层控件
                        m_mapControl.ActiveView.Refresh();
                        this.axTOCControl1.Refresh();
                    }
                }
            }
        }
    }
}
