using System;
using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using System.Collections.Generic;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;

namespace AEdeveloper.MyBaseTool
{
    /// <summary>
    /// Summary description for SearchAndZoomIn.
    /// </summary>
    [Guid("b6e64558-130d-4ae3-a6a0-e177338a0d1e")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("AEdeveloper.MyBaseTool.SearchAndZoomIn")]
    public sealed class SearchAndZoomIn : BaseTool
    {
        #region COM Registration Function(s)
        [ComRegisterFunction()]
        [ComVisible(false)]
        static void RegisterFunction(Type registerType)
        {
            // Required for ArcGIS Component Category Registrar support
            ArcGISCategoryRegistration(registerType);

            //
            // TODO: Add any COM registration code here
            //
        }

        [ComUnregisterFunction()]
        [ComVisible(false)]
        static void UnregisterFunction(Type registerType)
        {
            // Required for ArcGIS Component Category Registrar support
            ArcGISCategoryUnregistration(registerType);

            //
            // TODO: Add any COM unregistration code here
            //
        }

        #region ArcGIS Component Category Registrar generated code
        /// <summary>
        /// Required method for ArcGIS Component Category registration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryRegistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            MxCommands.Register(regKey);
            ControlsCommands.Register(regKey);
        }
        /// <summary>
        /// Required method for ArcGIS Component Category unregistration -
        /// Do not modify the contents of this method with the code editor.
        /// </summary>
        private static void ArcGISCategoryUnregistration(Type registerType)
        {
            string regKey = string.Format("HKEY_CLASSES_ROOT\\CLSID\\{{{0}}}", registerType.GUID);
            MxCommands.Unregister(regKey);
            ControlsCommands.Unregister(regKey);
        }

        #endregion
        #endregion

        private IHookHelper m_hookHelper = null;

        public SearchAndZoomIn()
        {
            //
            // TODO: Define values for the public properties
            //
            base.m_category = ""; //localizable text 
            base.m_caption = "";  //localizable text 
            base.m_message = "This should work in ArcMap/MapControl/PageLayoutControl";  //localizable text
            base.m_toolTip = "";  //localizable text
            base.m_name = "";   //unique id, non-localizable (e.g. "MyCategory_MyTool")
            try
            {
                //
                // TODO: change resource name if necessary
                //
                string bitmapResourceName = GetType().Name + ".bmp";
                base.m_bitmap = new Bitmap(GetType(), bitmapResourceName);
                base.m_cursor = new System.Windows.Forms.Cursor(GetType(), GetType().Name + ".cur");
            }
            catch (Exception ex)
            {
                System.Diagnostics.Trace.WriteLine(ex.Message, "Invalid Bitmap");
            }
        }

        #region Overridden Class Methods
       
        private IMapControl3 MyMapControl;
        private ILayer myselectedLayer = null;
        private List<IFeature> MyselectedFeature = null;
        public ILayer MyselectedLayer
        {
            get
            {
                for (int i = 0; i < MyMapControl.Map.LayerCount; i++)
                {
                    myselectedLayer = MyMapControl.Map.get_Layer(i);
                    if (myselectedLayer.Visible == true)
                        break;
                }

                return myselectedLayer;
            }
            set { myselectedLayer = value; }
        }
        /// <summary>
        /// Occurs when this tool is created
        /// </summary>
        /// <param name="hook">Instance of the application</param>
        public override void OnCreate(object hook)
        {
            try
            {
                m_hookHelper = new HookHelperClass();
                m_hookHelper.Hook = hook;
                MyMapControl = (IMapControl3)hook;
                if (m_hookHelper.ActiveView == null)
                {
                    m_hookHelper = null;
                }
            }
            catch
            {
                m_hookHelper = null;
            }

            if (m_hookHelper == null)
                base.m_enabled = false;
            else
                base.m_enabled = true;

            // TODO:  Add other initialization code
        }

        /// <summary>
        /// Occurs when this tool is clicked
        /// </summary>
        public override void OnClick()
        {
            // TODO: Add SearchAndZoomIn.OnClick implementation
        }

        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            // TODO:  Add SearchAndZoomIn.OnMouseDown implementation
            //空间查询
            try
            {
                MyMapControl.Map.ClearSelection();
                MyMapControl.Refresh();
                IArray geoArray = new ArrayClass();

                if (MyselectedLayer == null) return;
                IFeatureLayer featureLayer = MyselectedLayer as IFeatureLayer;
                if (featureLayer == null) return;
                IFeatureClass featureClass = featureLayer.FeatureClass;
                if (featureClass == null) return;

                IEnvelope envelope = MyMapControl.TrackRectangle();
                IGeometry geometry = envelope as IGeometry;
                ISpatialFilter spatialFilter = new SpatialFilterClass();
                spatialFilter.Geometry = geometry;//指定几何体
                switch (featureClass.ShapeType)
                {
                    case esriGeometryType.esriGeometryPoint:
                        spatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelContains;
                        break;
                    case esriGeometryType.esriGeometryPolygon:
                        spatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelIntersects;
                        break;
                    case esriGeometryType.esriGeometryPolyline:
                        spatialFilter.SpatialRel = esriSpatialRelEnum.esriSpatialRelCrosses;
                        break;
                }
                spatialFilter.GeometryField = featureClass.ShapeFieldName;

                IQueryFilter filter = spatialFilter as IQueryFilter;//筛选器

                IFeatureCursor cursor = featureClass.Search(filter, false);
                // 缩放到选择结果集，并高亮显示
                ZoomToSelectedFeature((IFeatureLayer)MyselectedLayer, filter);            
            }
            catch { }            
        }

        public override void OnMouseMove(int Button, int Shift, int X, int Y)
        {
            // TODO:  Add SearchAndZoomIn.OnMouseMove implementation
        }

        public override void OnMouseUp(int Button, int Shift, int X, int Y)
        {
            // TODO:  Add SearchAndZoomIn.OnMouseUp implementation
        }

        //高亮显示查询到的要素集合
        private void ZoomToSelectedFeature(IFeatureLayer pFeatureLyr, IQueryFilter pQueryFilter)
        {
            //符号边线颜色
            IRgbColor pLineColor = new RgbColor();
            pLineColor.Green = 130;
            ILineSymbol ilSymbl = new SimpleLineSymbolClass();
            ilSymbl.Color = pLineColor;
            ilSymbl.Width = 3;
            //定义选中要素的符号为红色
            ISimpleFillSymbol ipSimpleFillSymbol = new SimpleFillSymbol();
            ipSimpleFillSymbol.Outline = ilSymbl;
            RgbColor pFillColor = new RgbColor();
            pFillColor.Green = 60;
            ipSimpleFillSymbol.Color = pFillColor;
            ipSimpleFillSymbol.Style = esriSimpleFillStyle.esriSFSDiagonalCross;
            //选取要素集
            IFeatureSelection pFtSelection = pFeatureLyr as IFeatureSelection;
            pFtSelection.SetSelectionSymbol = true;
            pFtSelection.SelectionSymbol = (ISymbol)ipSimpleFillSymbol;
            pFtSelection.SelectFeatures(pQueryFilter, esriSelectionResultEnum.esriSelectionResultNew, false);

            ISelectionSet pSelectionSet = pFtSelection.SelectionSet;
            //居中显示选中要素
            IEnumGeometry pEnumGeom = new EnumFeatureGeometry();
            IEnumGeometryBind pEnumGeomBind = pEnumGeom as IEnumGeometryBind;
            pEnumGeomBind.BindGeometrySource(null, pSelectionSet);
            IGeometryFactory pGeomFactory = new GeometryEnvironmentClass();
            IGeometry pGeom = pGeomFactory.CreateGeometryFromEnumerator(pEnumGeom);
            MyMapControl.ActiveView.Extent = pGeom.Envelope;
            MyMapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
        }

        #endregion
    }
}
