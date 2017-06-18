using ESRI.ArcGIS.SystemUI;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Display;
using ESRI.ArcGIS.DataSourcesRaster;
using System;
using System.Windows.Forms;
using Crownwood.DotNetMagic.Docking;
using AEdeveloper.Control;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using AEdeveloper.MyBaseTool;
using ESRI.ArcGIS.GeoAnalyst;
using ESRI.ArcGIS.DataSourcesFile;
using System.Threading;

namespace AEdeveloper
{
    public partial class Mainform : Form
    {
        DockingManager _dockmanager1 = null;
        DockingManager _dockmanager2 = null;
        Crownwood.DotNetMagic.Docking.Content _workspaceContent = null;
        Crownwood.DotNetMagic.Docking.Content _workspaceContent2 = null;
        public MyMapContext _WorkSpaceCtrl;
        public ResourceManagement _Management;
        public Mainform()
        {
            InitializeComponent();
            InitGui();
            _WorkSpaceCtrl.axTOCControl1.SetBuddyControl(axMapControl1);//绑定控件
        }

        // 初始化程序界面
        private void InitGui()
        {
            _dockmanager1 = new DockingManager(this.panelmain, Crownwood.DotNetMagic.Common.VisualStyle.Office2007Silver);
            _dockmanager1.InnerControl = this.axMapControl1;
            _WorkSpaceCtrl = new MyMapContext(this);
            _WorkSpaceCtrl.Dock = DockStyle.Fill;
            _workspaceContent = _dockmanager1.Contents.Add(this._WorkSpaceCtrl);
            _dockmanager1.AddContentWithState(_workspaceContent, Crownwood.DotNetMagic.Docking.State.DockLeft);

            _dockmanager2 = new DockingManager(this.panelmain, Crownwood.DotNetMagic.Common.VisualStyle.Office2007Silver);
            _dockmanager2.InnerControl = this.axMapControl1;
            _Management = new ResourceManagement(this);
            _Management.Dock = DockStyle.Fill;
            _workspaceContent2 = _dockmanager2.Contents.Add(this._Management);
            _dockmanager2.AddContentWithState(_workspaceContent2, Crownwood.DotNetMagic.Docking.State.DockRight);
            IfControlEnabled();
        }

        //设置部分控件的可用性
        private void IfControlEnabled()
        {
            IFeatureLayer pfeaturelayer = null;
            for (int i = 0; i < this.axMapControl1.Map.LayerCount; i++)
            {
                pfeaturelayer = this.axMapControl1.Map.get_Layer(i) as IFeatureLayer;
                if (pfeaturelayer == null)
                    continue;
                else if (pfeaturelayer.Visible == true)
                    break;
            }
            if (pfeaturelayer != null)
            {
                button5.Enabled = true;
                button8.Enabled = true;
                button9.Enabled = true;
                button10.Enabled = true;
                button11.Enabled = true;
            }
            else
            {
                button5.Enabled = false;
                button8.Enabled = false;
                button9.Enabled = false;
                button10.Enabled = false;
                button11.Enabled = false;
            }
            button12.Enabled = false;
            button13.Enabled = false;
            button14.Enabled = false;
        }

        #region //start menu

        //退出按钮
        private void buttonItem13_Click(object sender, EventArgs e)
        {
            if (MessageBox.Show("退出程序前保存修改?", "确定", MessageBoxButtons.OKCancel, MessageBoxIcon.Question) == DialogResult.OK)
            {
                if (null != this.axMapControl1.DocumentFilename && this.axMapControl1.CheckMxFile(this.axMapControl1.DocumentFilename))
                {
                    // 创建一个新的地图文档实例 
                    IMapDocument mapDoc = new MapDocumentClass();
                    // 打开当前地图文档 
                    mapDoc.Open(this.axMapControl1.DocumentFilename, string.Empty);
                    // 保存地图文档 
                    mapDoc.Save(mapDoc.UsesRelativePaths, false);
                    mapDoc.Close();
                    MessageBox.Show("已保存更改!","确定", MessageBoxButtons.OK, MessageBoxIcon.Information) ;
                }
            }
            Application.Exit();
        }

        //打开地图Button
        private void OpenMapFile_Click(object sender, EventArgs e)
        {
            OpenMxdFile();
            IfControlEnabled();
        }

        //打开地图Item
        private void buttonItem3_Click(object sender, EventArgs e)
        {
            OpenMxdFile();
            IfControlEnabled();
        }

        //打开地图
        private void OpenMxdFile()
        {
            AboutOpenData OpenMapFile = new AboutOpenData();
            OpenMapFile.IfSaveTheCurrentMap(axMapControl1);
            OpenMapFile.Add_Any_layer("打开地图文档", "地图文档(*.*)|*.*", 6, axMapControl1);
        }

        //打开Mxd类型地图文档
        private void AddMxdFile_Click(object sender, EventArgs e)
        {
            AboutOpenData OpenMapFile = new AboutOpenData();
            OpenMapFile.IfSaveTheCurrentMap(axMapControl1);
            OpenMapFile.Add_Any_layer("打开地图文档", "地图文档(*.mxd)|*.mxd", 6, axMapControl1);
            IfControlEnabled();
        }
     
        //打开Pmf类型地图文档
        private void AddpmfFile_Click(object sender, EventArgs e)
        {
            AboutOpenData OpenMapFile = new AboutOpenData();
            OpenMapFile.IfSaveTheCurrentMap(axMapControl1);
            OpenMapFile.Add_Any_layer("打开地图文档", "地图文档(*.pmf)|*.pmf", 6, axMapControl1);
            IfControlEnabled();
        }

        //添加图层数据
        private void AddDataLayer_Click(object sender, EventArgs e)
        {
            //AboutOpenData OpenMapFile = new AboutOpenData();
            //OpenMapFile.IfSaveTheCurrentMap(axMapControl1);
            //OpenMapFile.Add_Any_layer("添加数据", "All Available File|*.*", 0, axMapControl1);
            IfControlEnabled();
        }

        //添加shapefile图层
        private void AddShapefile_Click(object sender, EventArgs e)
        {
            AboutOpenData OpenMapFile = new AboutOpenData();
            OpenMapFile.IfSaveTheCurrentMap(axMapControl1);
            OpenMapFile.Add_Any_layer("添加矢量数据", "ShapeFile|*.shp", 1, axMapControl1);
            IfControlEnabled(); 
        }

        //打开栅格数据
        private void AddRasterFile_Click(object sender, EventArgs e)
        {
            string rasterclass = "JPEG JPG File|*.jpg;*.jpeg|位图File|*.bmp|TIFF TIF File|*.tiff;*.tif|PNG File|*.png|EMF File|*.emf|AI File|*.ai|PDF文档|*.pdf|GIF File|*.gif|All Raster File|*.*";
            AboutOpenData OpenMapFile = new AboutOpenData();
            OpenMapFile.IfSaveTheCurrentMap(axMapControl1);
            OpenMapFile.Add_Any_layer("添加栅格数据", rasterclass, 2, axMapControl1);
            IfControlEnabled();
        }

        //添加lyr文件
        private void AddLayerDadaFile_Click(object sender, EventArgs e)
        {
            AboutOpenData OpenMapFile = new AboutOpenData();
            OpenMapFile.IfSaveTheCurrentMap(axMapControl1);
            OpenMapFile.Add_Any_layer("添加Lyr数据", "ArcGIS Lyr File|*.lyr", 3, axMapControl1);
            IfControlEnabled();
        }

        //添加个人数据库文件
        private void AddMdbFile_Click(object sender, EventArgs e)
        {
            AboutOpenData OpenMapFile = new AboutOpenData();
            OpenMapFile.IfSaveTheCurrentMap(axMapControl1);
            OpenMapFile.Add_Any_layer("添加个人数据库", "个人数据库|*.gdb|地理数据库|*.mdb", 4, axMapControl1);
            IfControlEnabled();
        }

        //add cad file
        private void AddCADFile_Click(object sender, EventArgs e)
        {
            AboutOpenData OpenMapFile = new AboutOpenData();
            OpenMapFile.IfSaveTheCurrentMap(axMapControl1);
            OpenMapFile.Add_Any_layer("添加CAD数据", "CAD File|*.cad", 5, axMapControl1);
            IfControlEnabled();
        }
        #endregion

        #region //鹰眼
        //当主地图显示控件的地图更换时，鹰眼中的地图也跟随更换
        private void axMapControl1_OnMapReplaced(object sender, IMapControlEvents2_OnMapReplacedEvent e)
        {
            if (this.axMapControl1.LayerCount > 0)
            {
                _WorkSpaceCtrl._SlightWorkSpaceCtrl.axMapControl1.Map = new MapClass();
                for (int i = 0; i <= this.axMapControl1.LayerCount - 1; i++)
                {
                    _WorkSpaceCtrl._SlightWorkSpaceCtrl.axMapControl1.AddLayer(this.axMapControl1.get_Layer(i));
                }
            }
            _WorkSpaceCtrl._SlightWorkSpaceCtrl.axMapControl1.Extent = this.axMapControl1.FullExtent;
            _WorkSpaceCtrl._SlightWorkSpaceCtrl.axMapControl1.Refresh();
        }
        //2.绘制鹰眼矩形框
        private void axMapControl1_OnExtentUpdated(object sender, IMapControlEvents2_OnExtentUpdatedEvent e)
        {
            // 得到新范围
            IEnvelope pEnv = (IEnvelope)e.newEnvelope;
            IGraphicsContainer pGra = _WorkSpaceCtrl._SlightWorkSpaceCtrl.axMapControl1.Map as IGraphicsContainer;
            IActiveView pAv = pGra as IActiveView;
            //在绘制前，清除axMapControl2中的任何图形元素
            pGra.DeleteAllElements();
            IRectangleElement pRectangleEle = new RectangleElementClass();
            IElement pEle = pRectangleEle as IElement;
            pEle.Geometry = pEnv;
            //设置鹰眼图中的红线框
            IRgbColor pColor = new RgbColorClass();
            pColor.Red = 0;
            pColor.Green = 0;
            pColor.Blue = 255;
            pColor.Transparency = 255;
            //产生一个线符号对象
            ILineSymbol pOutline = new SimpleLineSymbolClass();
            pOutline.Width = 1.5;
            pOutline.Color = pColor;
            //设置颜色属性
            pColor = new RgbColorClass();
            pColor.Red = 255;
            pColor.Green = 0;
            pColor.Blue = 0;
            pColor.Transparency = 0;
            //设置填充符号的属性
            IFillSymbol pFillSymbol = new SimpleFillSymbolClass();
            pFillSymbol.Color = pColor;
            pFillSymbol.Outline = pOutline;
            IFillShapeElement pFillShapeEle = pEle as IFillShapeElement;
            pFillShapeEle.Symbol = pFillSymbol;
            pGra.AddElement((IElement)pFillShapeEle, 0);
            pAv.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
        } 
        #endregion

        #region  //地图导航
        //中心放大
        private void button2_Click(object sender, EventArgs e)
        {
            double minX = this.axMapControl1.Extent.XMin;
            double maxX = this.axMapControl1.Extent.XMax;
            double minY = this.axMapControl1.Extent.YMin;
            double maxY = this.axMapControl1.Extent.YMax;
            double CenX, CenY, AbsX, AbsY;
            CenX = (minX + maxX) / 2; CenY = (minY + maxY) / 2;
            AbsX = maxX - minX; AbsY = maxY - minY;
            double tmpMinX = CenX - AbsX * 0.5 / 2;
            double tmpMinY = CenY - AbsY * 0.5 / 2;
            double tmpMaxX = CenX + AbsX * 0.5 / 2;
            double tmpMaxY = CenY + AbsY * 0.5 / 2;
            IEnvelope envelope = new Envelope() as IEnvelope;
            envelope.PutCoords(tmpMinX, tmpMinY, tmpMaxX, tmpMaxY);
            this.axMapControl1.Extent = envelope;
        }

        //中心缩小
        private void button3_Click(object sender, EventArgs e)
        {
            double minX = this.axMapControl1.Extent.XMin;
            double maxX = this.axMapControl1.Extent.XMax;
            double minY = this.axMapControl1.Extent.YMin;
            double maxY = this.axMapControl1.Extent.YMax;
            double CenX, CenY, AbsX, AbsY;
            CenX = (minX + maxX) / 2; CenY = (minY + maxY) / 2;
            AbsX = maxX - minX; AbsY = maxY - minY;
            double tmpMinX = CenX - AbsX;
            double tmpMinY = CenY - AbsY;
            double tmpMaxX = CenX + AbsX;
            double tmpMaxY = CenY + AbsY;
            IEnvelope envelope = new Envelope() as IEnvelope;
            envelope.PutCoords(tmpMinX, tmpMinY, tmpMaxX, tmpMaxY);
            this.axMapControl1.Extent = envelope;
        }

        //全图
        private void Extent_Click(object sender, EventArgs e)
        {
            this.axMapControl1.CurrentTool = null;
            this.axMapControl1.MousePointer = esriControlsMousePointer.esriPointerArrow;
            this.axMapControl1.Extent = this.axMapControl1.FullExtent;
        }

        //清空当前鼠标事件
        private void button1_Click(object sender, EventArgs e)
        {
            this.axMapControl1.CurrentTool = null;
            this.axMapControl1.MousePointer = esriControlsMousePointer.esriPointerArrow;
            IfControlEnabled();
        }

        //拉框放大
        private void button4_Click(object sender, EventArgs e)
        {
            this.axMapControl1.CurrentTool = null;
            TrackZoomIn mytoolTzoomIn = new TrackZoomIn();
            mytoolTzoomIn.OnCreate(this.axMapControl1.Object);
            this.axMapControl1.CurrentTool = mytoolTzoomIn;
            this.axMapControl1.MousePointer = esriControlsMousePointer.esriPointerPageZoomIn;
        }

        //移动地图
        private void button7_Click(object sender, EventArgs e)
        {
            this.axMapControl1.CurrentTool = null;
            ReMoveMap mytoolMovemap = new ReMoveMap();
            mytoolMovemap.OnCreate(this.axMapControl1.Object);
            this.axMapControl1.MousePointer = esriControlsMousePointer.esriPointerPan;
            this.axMapControl1.CurrentTool = mytoolMovemap;
        }
       
        //拉框缩小
        private void button6_Click(object sender, EventArgs e)
        {
            this.axMapControl1.MousePointer = esriControlsMousePointer.esriPointerPageZoomOut;
            this.axMapControl1.CurrentTool = null;
            TrackZoomOut mytoolTzoomOut = new TrackZoomOut();
            mytoolTzoomOut.OnCreate(this.axMapControl1.Object);
            this.axMapControl1.CurrentTool = mytoolTzoomOut;
        }

        //新建地图文档
        private void CreateNewMap_Click(object sender, EventArgs e)
        {
            //询问是否保存当前地图
            DialogResult res = MessageBox.Show("是否保存当前地图?", "提示", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (res == DialogResult.Yes)
            {
                //如果要保存，调用另存为对话框
                ICommand command = new ControlsSaveAsDocCommandClass();
                if (this.axMapControl1 != null)
                    command.OnCreate(this.axMapControl1.Object);
                command.OnClick();
            }
            //创建新的地图实例
            IMap map = new MapClass();
            map.Name = "Map";
            this.axMapControl1.DocumentFilename = string.Empty;
        }

        //Save Map
        private void savemap_Click(object sender, EventArgs e)
        {
            if (null != this.axMapControl1.DocumentFilename && this.axMapControl1.CheckMxFile(this.axMapControl1.DocumentFilename))
            {
                // 创建一个新的地图文档实例 
                IMapDocument mapDoc = new MapDocumentClass();
                // 打开当前地图文档 
                mapDoc.Open(this.axMapControl1.DocumentFilename, string.Empty);
                // 保存地图文档 
                mapDoc.Save(mapDoc.UsesRelativePaths, false);
                mapDoc.Close();
            }
        }

        //另存为
        private void SaveAsButton_Click(object sender, EventArgs e)
        {
            ICommand command = new ControlsSaveAsDocCommandClass();
            if (this.axMapControl1 != null)
                command.OnCreate(this.axMapControl1.Object);
            command.OnClick();
        }

        //上移地图
        private void upload_Click(object sender, EventArgs e)
        {
            double minX = this.axMapControl1.Extent.XMin;
            double maxX = this.axMapControl1.Extent.XMax;
            double minY = this.axMapControl1.Extent.YMin;
            double maxY = this.axMapControl1.Extent.YMax;
            double upremove = (maxY - minY) / 10;
            double tmpMinX = minX;
            double tmpMinY = minY - upremove;
            double tmpMaxX = maxX;
            double tmpMaxY = maxY - upremove;
            IEnvelope envelope = new Envelope() as IEnvelope;
            envelope.PutCoords(tmpMinX, tmpMinY, tmpMaxX, tmpMaxY);
            this.axMapControl1.Extent = envelope;
        }

        //下移地图
        private void download_Click(object sender, EventArgs e)
        {
            double minX = this.axMapControl1.Extent.XMin;
            double maxX = this.axMapControl1.Extent.XMax;
            double minY = this.axMapControl1.Extent.YMin;
            double maxY = this.axMapControl1.Extent.YMax;
            double Downremove = (maxY - minY) / 10;
            double tmpMinX = minX;
            double tmpMinY = minY + Downremove;
            double tmpMaxX = maxX;
            double tmpMaxY = maxY + Downremove;
            IEnvelope envelope = new Envelope() as IEnvelope;
            envelope.PutCoords(tmpMinX, tmpMinY, tmpMaxX, tmpMaxY);
            this.axMapControl1.Extent = envelope;
        }

        //左移地图
        private void leftload_Click(object sender, EventArgs e)
        {
            double minX = this.axMapControl1.Extent.XMin;
            double maxX = this.axMapControl1.Extent.XMax;
            double minY = this.axMapControl1.Extent.YMin;
            double maxY = this.axMapControl1.Extent.YMax;
            double Leftremove = (maxX - minX) / 10;
            double tmpMinX = minX + Leftremove;
            double tmpMinY = minY;
            double tmpMaxX = maxX + Leftremove;
            double tmpMaxY = maxY;
            IEnvelope envelope = new Envelope() as IEnvelope;
            envelope.PutCoords(tmpMinX, tmpMinY, tmpMaxX, tmpMaxY);
            this.axMapControl1.Extent = envelope;
        }

        //右移地图
        private void rightload_Click(object sender, EventArgs e)
        {
            double minX = this.axMapControl1.Extent.XMin;
            double maxX = this.axMapControl1.Extent.XMax;
            double minY = this.axMapControl1.Extent.YMin;
            double maxY = this.axMapControl1.Extent.YMax;
            double rightremove = (maxX - minX) / 10;
            double tmpMinX = minX - rightremove;
            double tmpMinY = minY;
            double tmpMaxX = maxX - rightremove;
            double tmpMaxY = maxY;
            IEnvelope envelope = new Envelope() as IEnvelope;
            envelope.PutCoords(tmpMinX, tmpMinY, tmpMaxX, tmpMaxY);
            this.axMapControl1.Extent = envelope;
        }

        #endregion

        #region //地图查询
 
        //属性查询
        private void button5_Click(object sender, EventArgs e)
        {
            SearchByAttribute frmSearch = new SearchByAttribute(this.axMapControl1);
            frmSearch.Show();
        }

        //拉框查询
        private void button8_Click(object sender, EventArgs e)
        {
            this.axMapControl1.CurrentTool = null;
            TrackRectangleQuary mytoolSpectialserach = new TrackRectangleQuary();
            mytoolSpectialserach.OnCreate(this.axMapControl1.Object);
            this.axMapControl1.CurrentTool = mytoolSpectialserach;
        }

        //点查询
        private void button9_Click(object sender, EventArgs e)
        {
            this.axMapControl1.CurrentTool = null;
            pPointSearch mytoolpPonitSearch = new pPointSearch();
            mytoolpPonitSearch.OnCreate(this.axMapControl1.Object);
            this.axMapControl1.CurrentTool = mytoolpPonitSearch;
        }

        //查询并缩放至
        private void button10_Click(object sender, EventArgs e)
        {
            this.axMapControl1.CurrentTool = null;
            SearchAndZoomIn mytoolSearchAndZoomIn = new SearchAndZoomIn();
            mytoolSearchAndZoomIn.OnCreate(this.axMapControl1.Object);
            this.axMapControl1.CurrentTool = mytoolSearchAndZoomIn;
        }
        #endregion

        #region //地图编辑

        private bool IsDoubleClicked = false;
        private bool EditingFeature = false;
        private bool Moving = false;
        private bool WaitingForUpdated = false;
        private bool VertexSnaped = false;
        private bool InnerSnaped = false;
        private IGeometry m_DisplayGeometry;
        private Edit _Editor;

        private void axMapControl1_OnMouseDown(object sender, IMapControlEvents2_OnMouseDownEvent e)
        {
            if (e.button == 2)
            {
                //弹出右键菜单
                _WorkSpaceCtrl.m_menuMap.PopupMenu(e.x, e.y, _WorkSpaceCtrl.m_mapControl.hWnd);
            }
            if (e.button == 1)
            {
                try
                {
                    if (VertexSnaped)
                    {
                        EditingFeature = true;
                        _Editor.EditFeature(e.x, e.y, m_DisplayGeometry);
                    }
                    else if (InnerSnaped)
                    {
                        if (WaitingForUpdated)
                        {
                            _Editor.ClearSelection();
                            _Editor.UpdateEdit(m_DisplayGeometry);
                            WaitingForUpdated = false;
                        }
                        else
                        {
                            _Editor.FeatureMoveMouseDown(e.x, e.y);
                            IsDoubleClicked = false;
                            Moving = true;
                        }
                        _Editor.ClearSelection();
                        _Editor.GetFeatureOnMouseDown(e.x, e.y);
                        _Editor.SelectOnMouseDown();
                        InnerSnaped = false;
                    }
                    else
                    {
                        if (_Editor.IsEditing)
                        {
                            if (WaitingForUpdated)
                            {
                                _Editor.UpdateEdit(m_DisplayGeometry);
                                WaitingForUpdated = false;
                                _Editor.ClearSelection();
                                _Editor.SelectOnMouseDown();
                            }
                            else
                            {
                                _Editor.ClearSelection();
                                _Editor.GetFeatureOnMouseDown(e.x, e.y);
                                _Editor.SelectOnMouseDown();
                                IsDoubleClicked = false;
                                InnerSnaped = false;
                                VertexSnaped = false;
                                m_DisplayGeometry = null;
                            }

                        }
                    }
                }
                catch { }
            } 
          
        }

        private void axMapControl1_OnMouseUp(object sender, IMapControlEvents2_OnMouseUpEvent e)
        {
            try
            {
                if (!_Editor.IsEditing) return;
                if (EditingFeature)
                {
                    m_DisplayGeometry = _Editor.EndFeatureEdit(e.x, e.y);
                    _Editor.ClearSelection();
                    _Editor.SelectOnMouseDown();
                    _Editor.SetMapcontrolMousePointer(esriControlsMousePointer.esriPointerArrow);
                    EditingFeature = false;
                    WaitingForUpdated = true;
                }
                else if (Moving)
                {
                    m_DisplayGeometry = _Editor.EndFeatureMove(e.x, e.y);
                    _Editor.UpdateEdit(m_DisplayGeometry);
                    _Editor.ClearSelection();
                    _Editor.SelectOnMouseDown();
                    Moving = false;
                    WaitingForUpdated = true;
                }
            }
            catch { }
        }

        private void axMapControl1_OnDoubleClick(object sender, IMapControlEvents2_OnDoubleClickEvent e)
        {
            try
            {
                if (!_Editor.IsEditing) return;
                m_DisplayGeometry = _Editor.MouseClickGeometry;
                _Editor.SelectOnMouseDown();
                IsDoubleClicked = true;
            }
            catch { }
        }

        private void axMapControl1_OnMouseMove(object sender, IMapControlEvents2_OnMouseMoveEvent e)
        {
            // 显示当前比例尺 
            ScaleLabel.Text = " 比例尺 1:" + ((long)this.axMapControl1.MapScale).ToString();
            // 显示当前坐标
            CoordinateLabel.Text = " 当前坐标 X = " + e.mapX.ToString() + " Y = " + e.mapY.ToString() + " " + this.axMapControl1.MapUnits.ToString().Substring(4);
            try
            {
                if (!_Editor.IsEditing) return;
                if (Moving)
                {
                    _Editor.FeatureMoveMouseMove(e.x, e.y);
                }
                else if (EditingFeature)
                {
                    _Editor.FeatureEditMouseMove(e.x, e.y);
                }
                else if (IsDoubleClicked)
                {
                    _Editor.SnapVertex(e.x, e.y, m_DisplayGeometry, ref VertexSnaped, ref InnerSnaped);
                }
                else _Editor.SetMapcontrolMousePointer(esriControlsMousePointer.esriPointerArrow);
            }
            catch { }
        }

        private void axMapControl1_OnAfterDraw(object sender, IMapControlEvents2_OnAfterDrawEvent e)
        {
            try
            {
                if (!_Editor.IsEditing) return;
                esriViewDrawPhase phase = (esriViewDrawPhase)e.viewDrawPhase;
                if (phase == esriViewDrawPhase.esriViewForeground)
                {
                    if (IsDoubleClicked)
                    {
                        if (m_DisplayGeometry == null) return;
                        _Editor.DrawEditSymbol(m_DisplayGeometry, e.display as IDisplay);
                    }
                }
            }
            catch { }
        }

        private void button11_Click(object sender, EventArgs e)
        {
            _Editor = new Edit(this.axMapControl1);
            if (_Editor.IsEditing) return;
            _Editor.StartEditing(true);
            button12.Enabled = true;
            button13.Enabled = true;
            button14.Enabled = true;
        }

        private void button12_Click(object sender, EventArgs e)
        {
            if (!_Editor.IsEditing) return;
            _Editor.StopEditing(true);
            button13.Enabled = false;
            button14.Enabled = false;
            button12.Enabled = false;
        }

        private void button13_Click(object sender, EventArgs e)
        {
            if (!_Editor.IsEditing) return;
            _Editor.UndoEdit();
        }

        private void button14_Click(object sender, EventArgs e)
        {
            if (!_Editor.IsEditing) return;
            _Editor.RedoEdit();
        }

        #endregion
        private void ConventALot_Click(object sender, EventArgs e)
        {
            System.Windows.Forms.OpenFileDialog openfiledialog;
            openfiledialog = new OpenFileDialog();
            string dialogname = "打开矢量数据";
            string fileclass = "ShapeFile|*.shp";
            openfiledialog.Title = dialogname;
            openfiledialog.Filter = fileclass;

            if (openfiledialog.ShowDialog() == DialogResult.OK)
            {
                string fullfilepath = openfiledialog.FileName;//存储打开文件的全路径
                Thread pThread = new Thread(new ParameterizedThreadStart(ConventAlot));
                pThread.IsBackground = true;
                pThread.Start(fullfilepath);
            }

        }
        void ConventAlot(object data)
        {
            string fullfilepath = data as string;//存储打开文件的全路径
            int index = fullfilepath.LastIndexOf("\\");
            int index1 = fullfilepath.LastIndexOf(".");
            string filepath = fullfilepath.Substring(0, index); //获得文件路径
            string filename = fullfilepath.Substring(index + 1);//获得文件名称 
            string nameonly = fullfilepath.Substring(index+1, index1-index-1);//不含后缀名
            IWorkspaceFactory pWorkSpaceFac = new ShapefileWorkspaceFactoryClass();
            IFeatureWorkspace pFeatureWorkSpace = pWorkSpaceFac.OpenFromFile(filepath, 0) as IFeatureWorkspace;
            IFeatureClass oldFeatureClass = pFeatureWorkSpace.OpenFeatureClass(filename);
            IFeatureLayer oldFeatureLayer = new FeatureLayer();
            oldFeatureLayer.FeatureClass = oldFeatureClass;            
            IFeatureCursor pFeatureCursor = oldFeatureLayer.Search(null, true);
            IFeature pFeature = pFeatureCursor.NextFeature();
            IFields pFields = pFeature.Fields; ;
            IFeatureClass newFeatureClass;
            string fileName = "temp.shp";
            if (System.IO.File.Exists(filepath+"\\"+fileName))
                newFeatureClass = pFeatureWorkSpace.OpenFeatureClass(fileName);
            else 
                newFeatureClass = pFeatureWorkSpace.CreateFeatureClass(fileName, pFields, null, null, esriFeatureType.esriFTSimple, "Shape",null);
            while(pFeature!=null)
            {
                string RasterName = nameonly + pFeature.get_Value(0).ToString();
                string fullname = filepath +"\\"+ RasterName + ".tif";
                if (!System.IO.File.Exists(fullname))
                {
                    IFeatureClass pFeatureClassNew = AddFeatureToFeatureClass(newFeatureClass, pFeature);
                    ((IFeatureClassManage)pFeatureClassNew).UpdateExtent();
                    FeatureToRaster(pFeatureClassNew, "perimeter", 30, filepath, RasterName);
                    //删除先前加入的要素
                    ITable dTable = newFeatureClass as ITable;
                    dTable.DeleteSearchedRows(null);     
                }
                pFeature = pFeatureCursor.NextFeature();
            }
        }
        /// <summary>
        /// 矢量转栅格
        /// </summary>
        /// <param name="IFeatureClass">要转换的矢量数据</param>
        /// <param name="eleField">转换的字段名</param>
        /// <param name="cellsize">栅格大小,默认为null</param>
        ///<param name="rasterSavePath">保存路径</param>
        ///<param name="demName">dem名称</param>
        /// <returns>返回ILayer类型的图层</returns>
        public ILayer FeatureToRaster(IFeatureClass pFeatureClass, string eleField, object cellSize, string rasterSavePath, string demName)
        {
            IFeatureClassDescriptor pFeatureClassDescriptor = new FeatureClassDescriptorClass();//获取转化的字段
            pFeatureClassDescriptor.Create(pFeatureClass, null, eleField);//转换字段
            IGeoDataset pGeoDataset = (IGeoDataset)pFeatureClassDescriptor;//获取第一个参数
            //默认栅格大小
            if (Convert.ToDouble(cellSize) <= 0)
            {
                IEnvelope envelope = pGeoDataset.Extent;
                if (envelope.Width > envelope.Height)
                {    cellSize = envelope.Height / 250;}
                else{   cellSize = envelope.Width / 250;}
            }
            IWorkspaceFactory workspaceFactory = new RasterWorkspaceFactoryClass();
            IRasterWorkspace2 rasterWorkspace2 = workspaceFactory.OpenFromFile(rasterSavePath, 0) as IRasterWorkspace2;
            //in case that there is already an existing raster with the raster name, try to delete it
            if (System.IO.Directory.Exists(System.IO.Path.Combine(rasterSavePath, demName)))
            {
                IDataset dataset = rasterWorkspace2.OpenRasterDataset(demName) as IDataset;
                dataset.Delete();
            }
            IConversionOp conversionOp = new RasterConversionOpClass();
            IRasterAnalysisEnvironment rasterAnalysisEnvironment = conversionOp as IRasterAnalysisEnvironment;
            rasterAnalysisEnvironment.SetCellSize(esriRasterEnvSettingEnum.esriRasterEnvValue, ref cellSize);
            IRasterDataset rasterDataset = null;
            try
            {     rasterDataset = conversionOp.ToRasterDataset(pGeoDataset, "TIFF", (IWorkspace)rasterWorkspace2, demName);}
            catch (Exception ee)
            {     MessageBox.Show(ee.Message); }
            ILayer layer = null;
            if (rasterDataset != null)
            {
                IRasterLayer pRlyr = new RasterLayerClass();
                pRlyr.CreateFromDataset(rasterDataset);
                layer = pRlyr as ILayer;
            }
            return layer;
        }      
        private IFeatureClass AddFeatureToFeatureClass(IFeatureClass pFeatureClass, IFeature pFeature)
        {
            IFeatureCursor pFeatureCursor = pFeatureClass.Insert(true);
            IFeatureBuffer pFeatureBuffer = pFeatureClass.CreateFeatureBuffer();
            IFields pFields = pFeatureClass.Fields;
            for (int i = 1; i <= pFeature.Fields.FieldCount - 1; i++)
            {
                IField pField = pFields.get_Field(i);
                if (pField.Type == esriFieldType.esriFieldTypeGeometry)
                {
                    pFeatureBuffer.set_Value(i, pFeature.ShapeCopy);
                }
                else
                {   // try// {
                    switch (pField.Type)
                    {
                        case esriFieldType.esriFieldTypeInteger:
                            pFeatureBuffer.set_Value(i, Convert.ToInt32(pFeature.get_Value(i)));
                            break;
                        case esriFieldType.esriFieldTypeDouble:
                            pFeatureBuffer.set_Value(i, Convert.ToDouble(pFeature.get_Value(i)));
                            break;
                        case esriFieldType.esriFieldTypeString:
                            pFeatureBuffer.set_Value(i, Convert.ToString(pFeature.get_Value(i)));
                            break;
                        default:
                            pFeatureBuffer.set_Value(i, Convert.ToDouble(pFeature.get_Value(i)));
                            break;// }
                    } // catch (Exception e)// { // }
                }
            }
            pFeatureCursor.InsertFeature(pFeatureBuffer);
            return pFeatureClass;
        }
    }

}
