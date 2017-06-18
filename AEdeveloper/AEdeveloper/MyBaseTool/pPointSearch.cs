using System;
using System.Drawing;
using System.Runtime.InteropServices;
using ESRI.ArcGIS.ADF.BaseClasses;
using ESRI.ArcGIS.ADF.CATIDs;
using ESRI.ArcGIS.Controls;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using System.Collections.Generic;
using ESRI.ArcGIS.esriSystem;
using ESRI.ArcGIS.Geometry;

namespace AEdeveloper.MyBaseTool
{
    /// <summary>
    /// Summary description for pPointSearch.
    /// </summary>
    [Guid("9ea0d46d-844d-4b64-9e97-516bffbe73a6")]
    [ClassInterface(ClassInterfaceType.None)]
    [ProgId("AEdeveloper.MyBaseTool.pPointSearch")]
    public sealed class pPointSearch : BaseTool
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

        /// <param name="hook">Instance of the application</param>
        private IMapControl3 MyMapControl;
        private ILayer myselectedLayer = null;
        private List<IFeature> MyselectedFeature = null;
        private IHookHelper m_hookHelper = null;

        public pPointSearch()
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
            // TODO: Add pPointSearch.OnClick implementation
        }

        public override void OnMouseDown(int Button, int Shift, int X, int Y)
        {
            // TODO:  Add pPointSearch.OnMouseDown implementation
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
                //IEnvelope envelope = MyMapControl.TrackRectangle();
                ISpatialFilter spatialFilter = new SpatialFilterClass();
                IPoint point = MyMapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(X, Y);
                IGeometry geometry = point as IGeometry;
                double length = ConvertPixelsToMapUnits(4);
                ITopologicalOperator pTopo = geometry as ITopologicalOperator;
                IGeometry buffer = pTopo.Buffer(length);//获取缓冲区
                geometry = buffer.Envelope as IGeometry;

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

                IFeature pfeature = cursor.NextFeature();
                MyselectedFeature = new List<IFeature>();//此处**初始化list
                while (pfeature != null)
                {
                    geoArray.Add(pfeature);//直接存入数组geoArray用于闪烁 
                    MyselectedFeature.Add(pfeature); //存入list
                    pfeature = cursor.NextFeature(); //枚举
                }
                //或者将list（MyselectedFeature）转存入数组geoArray用于闪烁 
                /*for (int i = 0; i < MyselectedFeature.Count; i++)
                {
                    geoArray.Add(MyselectedFeature[i]);
                }*/
                SelectOnMouseDown();//高亮
                FlashAndIdentify(geoArray);//闪烁
            }
            catch { }
        }

        private void FlashAndIdentify(IArray inArray)
        {
            try
            {
                if (inArray == null)
                    return;
                HookHelperClass hookHelper = new HookHelperClass();
                hookHelper.Hook = MyMapControl.Object;
                IHookActions hookAction = (IHookActions)hookHelper;
                hookAction.DoActionOnMultiple(inArray, esriHookActions.esriHookActionsFlash);
            }
            catch (Exception exc) { MessageBox.Show(exc.Message); }
        }
        public override void OnMouseMove(int Button, int Shift, int X, int Y)
        {
            // TODO:  Add pPointSearch.OnMouseMove implementation
        }

        public override void OnMouseUp(int Button, int Shift, int X, int Y)
        {
            // TODO:  Add pPointSearch.OnMouseUp implementation
        }

        // 根据鼠标点击位置使击中要素处于高亮显示状态
        public void SelectOnMouseDown()
        {
            try
            {
                if (MyselectedLayer == null) return;
                MyMapControl.Map.ClearSelection();
                MyMapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
                foreach (IFeature feature in MyselectedFeature.ToArray())
                {
                    MyMapControl.Map.SelectFeature(MyselectedLayer, feature);
                }
                MyMapControl.Extent = MyselectedFeature[0].Extent;
                MyMapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
            }
            catch { return; }
        }
        //控制选择范围
        private double ConvertPixelsToMapUnits(double pixelUnits)
        {
            int pixelExtent = MyMapControl.ActiveView.ScreenDisplay.DisplayTransformation.get_DeviceFrame().right
                           - MyMapControl.ActiveView.ScreenDisplay.DisplayTransformation.get_DeviceFrame().left;

            double realWorldDisplayExtent = MyMapControl.ActiveView.ScreenDisplay.DisplayTransformation.VisibleBounds.Width;
            double sizeOfOnePixel = realWorldDisplayExtent / pixelExtent;

            return pixelUnits * sizeOfOnePixel;
        }
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
        #endregion
    }
}
