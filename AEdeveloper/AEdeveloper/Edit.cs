using System;
using System.Collections.Generic;
using System.Text;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Display;

namespace AEdeveloper
{
    class Edit
    {
        private AxMapControl MyMapControl;//地图空间
        private ILayer MyselectedLayer;//选中的图层
        private bool If_isEdited = false;//是否处于编辑状态
        private bool If_isInUse = false;//是否被使用
        private List<IFeature> MyselectedFeature;//选中的要素
        private IPoint MycurrentMousePosition;//鼠标当前位置
        private IDisplayFeedback MyfeedBack;//显示结果
        private IPointCollection MypointCollection;//点集
        //范围region 

        #region 属性

        public AxMapControl EditedMap
        {
            get { return MyMapControl; }
            set { MyMapControl = value; }
        }

        public ILayer CurrentLayer
        {
            get { return MyselectedLayer; }
            set { MyselectedLayer = value; }
        }

        /// <summary>
        /// 判断是否处于编辑状态
        /// </summary>
        public bool IsEditing
        {
            get { return If_isEdited; }
        }

        public List<IFeature> SelectedFeature
        {
            get { return MyselectedFeature; }
        }

        public Edit()
        {
            MyMapControl = null;
            MyselectedLayer = null;
            MyselectedFeature = new List<IFeature>();
            MycurrentMousePosition = null;
            MyfeedBack = null;
            MypointCollection = null;
        }

        public Edit(AxMapControl editedMap)
        {
            MyMapControl = editedMap;
            for (int i = 0; i < editedMap.Map.LayerCount; i++)
            {
                MyselectedLayer = editedMap.Map.get_Layer(i);
                if (MyselectedLayer == null)
                    continue;
                else if (MyselectedLayer.Visible == true)
                    break;
            }
            MyselectedFeature = new List<IFeature>();
            MycurrentMousePosition = null;
            MyfeedBack = null;
            MypointCollection = null;
        }

        public IGeometry MouseClickGeometry
        {
            get
            {
                if (MyselectedFeature.Count > 0)
                {
                    return MyselectedFeature[0].Shape;
                }
                else return null;
            }
        }
        #endregion

        #region MapControl显示控制
        /// <summary>
        /// 设置鼠标样式
        /// </summary>
        /// <param name="pointer"></param>
        public void SetMapcontrolMousePointer(esriControlsMousePointer pointer)
        {
            MyMapControl.MousePointer = pointer;
        }

        /// <summary>
        /// 清除要素选择状态，恢复常态
        /// </summary>
        public void ClearSelection()
        {
            MyMapControl.Map.ClearSelection();
            MyMapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, MyselectedLayer, null);
        }

        /// <summary>
        /// 在要素上面绘制一个可拖拽的符号
        /// </summary>
        /// <param name="geometry"></param>
        public void DrawEditSymbol(IGeometry geometry, IDisplay display)
        {
            IEngineEditProperties engineProperty = new EngineEditorClass();

            ISymbol pointSymbol = engineProperty.SketchVertexSymbol as ISymbol;
            ISymbol sketchSymbol = engineProperty.SketchSymbol as ISymbol;

            ITopologicalOperator pTopo = geometry as ITopologicalOperator;

            sketchSymbol.SetupDC(display.hDC, display.DisplayTransformation);
            sketchSymbol.Draw(pTopo.Boundary);

            IPointCollection pointCol = geometry as IPointCollection;
            for (int i = 0; i < pointCol.PointCount; i++)
            {
                IPoint point = pointCol.get_Point(i);
                pointSymbol.SetupDC(display.hDC, display.DisplayTransformation);
                pointSymbol.Draw(point);
                pointSymbol.ResetDC();
            }

            //mMyMapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, mMyselectedLayer, null);
            //-----
            //ESRI.ArcGIS.ADF.ComReleaser.ReleaseCOMObject(sketchSymbol);
            //ESRI.ArcGIS.ADF.ComReleaser.ReleaseCOMObject(pointSymbol);
            //ESRI.ArcGIS.ADF.ComReleaser.ReleaseCOMObject(engineProperty);
        }
        #endregion

        #region 开始、结束编辑
        /// <summary>
        /// 开始编辑
        /// </summary>
        /// <param name="bWithUndoRedo"></param>
        public void StartEditing(bool WithUndoRedo)
        {
            if (MyselectedLayer == null) return;
            IFeatureLayer featureLayer = MyselectedLayer as IFeatureLayer;
            if (featureLayer == null) return;
            IFeatureClass featureClass = featureLayer.FeatureClass;
            if (featureClass == null) return;

            IDataset dataset = featureClass as IDataset;
            IWorkspaceEdit workspaceEdit = dataset.Workspace as IWorkspaceEdit;
            try
            {
                workspaceEdit.StartEditing(WithUndoRedo);
                If_isEdited = true;
            }
            catch
            {
                return;
            }
        }

        /// <summary>
        /// 结束编辑
        /// </summary>
        /// <param name="Save"></param>
        public void StopEditing(bool Save)
        {
            if (If_isEdited)
            {
                If_isEdited = false;

                if (MyselectedLayer == null) return;
                IFeatureLayer featureLayer = MyselectedLayer as IFeatureLayer;
                if (featureLayer == null) return;
                IFeatureClass featureClass = featureLayer.FeatureClass;
                if (featureClass == null) return;

                IDataset dataset = featureClass as IDataset;
                IWorkspaceEdit workspaceEdit = dataset.Workspace as IWorkspaceEdit;
                if (workspaceEdit.IsBeingEdited())
                {
                    try
                    {
                        workspaceEdit.StopEditing(Save);
                    }
                    catch
                    {
                        workspaceEdit.AbortEditOperation();
                        return;
                    }
                }
            }
        }
        #endregion

        #region 选择要素，使其处于高亮状态
        public void GetFeatureOnMouseDown(int x, int y)
        {
            MyselectedFeature.Clear();
            try
            {
                if (MyselectedLayer == null) return;
                IFeatureLayer featureLayer = MyselectedLayer as IFeatureLayer;
                if (featureLayer == null) return;
                IFeatureClass featureClass = featureLayer.FeatureClass;
                if (featureClass == null) return;

                IPoint point = MyMapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
                IGeometry geometry = point as IGeometry;

                double length = ConvertPixelsToMapUnits(4);
                ITopologicalOperator pTopo = geometry as ITopologicalOperator;
                IGeometry buffer = pTopo.Buffer(length);
                geometry = buffer.Envelope as IGeometry;

                ISpatialFilter spatialFilter = new SpatialFilterClass();
                spatialFilter.Geometry = geometry;
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
                IQueryFilter filter = spatialFilter as IQueryFilter;

                IFeatureCursor cursor = featureClass.Search(filter, false);
                IFeature pfeature = cursor.NextFeature();
                while (pfeature != null)
                {
                    MyselectedFeature.Add(pfeature);
                    pfeature = cursor.NextFeature();
                }
            }
            catch
            {
                return;
            }
        }

        /// <summary>
        /// 根据鼠标点击位置使击中要素处于高亮显示状态
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
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
                IGeometry pGeom = MouseClickGeometry.Envelope;
                MyMapControl.ActiveView.Extent = pGeom.Envelope;
                MyMapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeoSelection, null, null);
            }
            catch
            { return; }
        }
        #endregion

        #region 绘制新图形

        public void SketchMouseDown(int x, int y)
        {
            if (MyselectedLayer == null) return;
            if ((MyselectedLayer as IGeoFeatureLayer) == null) return;

            IFeatureLayer featureLayer = MyselectedLayer as IFeatureLayer;
            IFeatureClass featureClass = featureLayer.FeatureClass;
            if (featureClass == null) return;

            IPoint point = MyMapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
            INewLineFeedback lineFeedback = null;
            INewPolygonFeedback polygonFeedback = null;
            try
            {
                if (!If_isInUse)
                {
                    switch (featureClass.ShapeType)
                    {
                        case esriGeometryType.esriGeometryPoint:
                            break;
                        case esriGeometryType.esriGeometryMultipoint:
                            If_isInUse = true;
                            MyfeedBack = new NewMultiPointFeedbackClass();
                            INewMultiPointFeedback multiPointFeedback = MyfeedBack as INewMultiPointFeedback;
                            MypointCollection = new MultipointClass();
                            multiPointFeedback.Start(MypointCollection, point);
                            break;
                        case esriGeometryType.esriGeometryPolyline:
                            If_isInUse = true;
                            MyfeedBack = new NewLineFeedbackClass();
                            lineFeedback = MyfeedBack as INewLineFeedback;
                            lineFeedback.Start(point);
                            break;
                        case esriGeometryType.esriGeometryPolygon:
                            If_isInUse = true;
                            MyfeedBack = new NewPolygonFeedbackClass();
                            polygonFeedback = MyfeedBack as INewPolygonFeedback;
                            polygonFeedback.Start(point);
                            break;
                        default:
                            break;
                    }
                }
                else
                {
                    if ((MyfeedBack as INewMultiPointFeedback) != null)
                    {
                        object missing = Type.Missing;
                        MypointCollection.AddPoint(point, ref missing, ref missing);
                    }
                    else if ((MyfeedBack as INewLineFeedback) != null)
                    {
                        lineFeedback = MyfeedBack as INewLineFeedback;
                        lineFeedback.AddPoint(point);
                    }
                    else if ((MyfeedBack as INewPolygonFeedback) != null)
                    {
                        polygonFeedback = MyfeedBack as INewPolygonFeedback;
                        polygonFeedback.AddPoint(point);
                    }
                }
            }
            catch { return; }
        }

        public void SketchMouseMove(int x, int y)
        {
            if ((!If_isInUse) || (MyfeedBack == null)) return;

            MycurrentMousePosition = MyMapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
            MyfeedBack.MoveTo(MycurrentMousePosition);
        }

        public void EndSketch()
        {
            IGeometry resGeometry = null;
            IPointCollection pointColl = null;

            try
            {
                if ((MyfeedBack as INewMultiPointFeedback) != null)
                {
                    INewMultiPointFeedback multiPointFeedback = MyfeedBack as INewMultiPointFeedback;
                    multiPointFeedback.Stop();
                }
                else if ((MyfeedBack as INewLineFeedback) != null)
                {
                    INewLineFeedback lineFeedback = MyfeedBack as INewLineFeedback;
                    lineFeedback.AddPoint(MycurrentMousePosition);
                    IPolyline polyline = lineFeedback.Stop();
                    pointColl = polyline as IPointCollection;
                    if (pointColl.PointCount > 1) resGeometry = pointColl as IGeometry;
                }
                else if ((MyfeedBack as INewPolygonFeedback) != null)
                {
                    INewPolygonFeedback polygonFeedback = MyfeedBack as INewPolygonFeedback;
                    polygonFeedback.AddPoint(MycurrentMousePosition);
                    IPolygon polygon = polygonFeedback.Stop();
                    if (polygon != null)
                    {
                        pointColl = polygon as IPointCollection;
                        if (pointColl.PointCount > 2)
                        {
                            resGeometry = pointColl as IGeometry;
                            ITopologicalOperator pTopo = resGeometry as ITopologicalOperator;
                            if (!pTopo.IsKnownSimple) pTopo.Simplify();
                        }
                    }
                }
                MyfeedBack = null;
                If_isInUse = false;
            }
            catch { return; }
        }
        #endregion

        #region 编辑要素节点
        public void SnapVertex(int x, int y, IGeometry snapContainer, ref bool vertexSnaped, ref bool contained)
        {
            IPoint point = MyMapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
            IPoint pHitPoint = null;
            double hitDist = -1, tol = -1;
            int vertexIndex = -1, partIndex = -1;
            bool vertex = false;

            tol = ConvertPixelsToMapUnits(4);

            IHitTest pHitTest = snapContainer as IHitTest;
            bool bHit = pHitTest.HitTest(point, tol, esriGeometryHitPartType.esriGeometryPartVertex, pHitPoint, ref hitDist, ref partIndex, ref vertexIndex, ref vertex);
            vertexSnaped = false;
            contained = false;
            if (bHit)
            {
                MyMapControl.MousePointer = esriControlsMousePointer.esriPointerCrosshair;
                vertexSnaped = true;
                return;
            }
            else
            {
                IRelationalOperator pRelOperator = null;
                ITopologicalOperator pTopo = null;
                IGeometry buffer = null;
                IPolygon polygon = null;
                switch (snapContainer.GeometryType)
                {
                    case esriGeometryType.esriGeometryPolyline:
                        pTopo = snapContainer as ITopologicalOperator;
                        buffer = pTopo.Buffer(3);
                        polygon = buffer as IPolygon;
                        pRelOperator = polygon as IRelationalOperator;
                        break;
                    case esriGeometryType.esriGeometryPolygon:
                        polygon = snapContainer as IPolygon;
                        pRelOperator = polygon as IRelationalOperator;
                        break;
                    case esriGeometryType.esriGeometryPoint:
                        pTopo = snapContainer as ITopologicalOperator;
                        buffer = pTopo.Buffer(3);
                        polygon = buffer as IPolygon;
                        pRelOperator = polygon as IRelationalOperator;
                        break;
                    default:
                        break;
                }

                if (pRelOperator == null) return;
                if (pRelOperator.Contains(point))
                {
                    MyMapControl.MousePointer = esriControlsMousePointer.esriPointerSizeAll;
                    contained = true;
                }
                else MyMapControl.MousePointer = esriControlsMousePointer.esriPointerArrow;
                return;
            }
        }

        public bool EditFeature(int x, int y, IGeometry geometry)
        {
            GetFeatureOnMouseDown(x, y);
            SelectOnMouseDown();
            if (MyselectedFeature.Count < 1) return false;
            if (geometry == null) return false;

            IPoint pHitPoint = null;
            double hitDist = 0, tol = 0;
            int vertexIndex = 0, vertexOffset = 0, numVertices = 0, partIndex = 0;
            bool vertex = false;

            IFeature editedFeature = MyselectedFeature[0];
            IPoint point = MyMapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
            tol = ConvertPixelsToMapUnits(4);
            //IGeometry pGeo = editedFeature.Shape;
            //m_EditingFeature = editedFeature;

            try
            {
                switch (geometry.GeometryType)
                {
                    case esriGeometryType.esriGeometryPoint:
                        MyfeedBack = new MovePointFeedbackClass();
                        MyfeedBack.Display = MyMapControl.ActiveView.ScreenDisplay;
                        IMovePointFeedback pointMove = MyfeedBack as IMovePointFeedback;
                        pointMove.Start(geometry as IPoint, point);
                        break;
                    case esriGeometryType.esriGeometryPolyline:
                        if (TestGeometryHit(tol, point, geometry, ref pHitPoint, ref hitDist, ref partIndex, ref vertexOffset, ref vertexIndex, ref vertex))
                        {
                            if (!vertex)
                            {
                                IGeometryCollection geometryColl = geometry as IGeometryCollection;
                                IPath path = geometryColl.get_Geometry(partIndex) as IPath;
                                IPointCollection pointColl = path as IPointCollection;
                                numVertices = pointColl.PointCount;
                                object missing = Type.Missing;

                                if (vertexIndex == 0)
                                {
                                    object start = 1 as object;
                                    pointColl.AddPoint(point, ref start, ref missing);
                                }
                                else
                                {
                                    object objVertexIndex = vertexIndex as object;
                                    pointColl.AddPoint(point, ref missing, ref objVertexIndex);
                                }
                                TestGeometryHit(tol, point, geometry, ref pHitPoint, ref hitDist, ref partIndex, ref vertexOffset, ref vertexIndex, ref vertex);
                            }
                            MyfeedBack = new LineMovePointFeedbackClass();
                            MyfeedBack.Display = MyMapControl.ActiveView.ScreenDisplay;
                            ILineMovePointFeedback lineMove = MyfeedBack as ILineMovePointFeedback;
                            lineMove.Start(geometry as IPolyline, vertexIndex, point);
                        }
                        else return false;
                        break;
                    case esriGeometryType.esriGeometryPolygon:
                        if (TestGeometryHit(tol, point, geometry, ref pHitPoint, ref hitDist, ref partIndex, ref vertexOffset, ref vertexIndex, ref vertex))
                        {
                            if (!vertex)
                            {
                                IGeometryCollection geometryColl = geometry as IGeometryCollection;
                                IPath path = geometryColl.get_Geometry(partIndex) as IPath;
                                IPointCollection pointColl = path as IPointCollection;
                                numVertices = pointColl.PointCount;
                                object missing = Type.Missing;
                                if (vertexIndex == 0)
                                {
                                    object start = 1 as object;
                                    pointColl.AddPoint(point, ref start, ref missing);
                                }
                                else
                                {
                                    object objVertexIndex = vertexIndex as object;
                                    pointColl.AddPoint(point, ref missing, ref objVertexIndex);
                                }
                                TestGeometryHit(tol, point, geometry, ref pHitPoint, ref hitDist, ref partIndex, ref vertexOffset, ref vertexIndex, ref vertex);
                            }
                            MyfeedBack = new PolygonMovePointFeedbackClass();
                            MyfeedBack.Display = MyMapControl.ActiveView.ScreenDisplay;
                            IPolygonMovePointFeedback polyMove = MyfeedBack as IPolygonMovePointFeedback;
                            polyMove.Start(geometry as IPolygon, vertexIndex + vertexOffset, point);
                        }
                        else return false;
                        break;
                    default:
                        break;
                }
            }
            catch { return false; }
            return true;
        }

        public void FeatureEditMouseMove(int x, int y)
        {
            if (MyfeedBack == null) return;
            IPoint point = MyMapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
            MyfeedBack.MoveTo(point);
        }

        public IGeometry EndFeatureEdit(int x, int y)
        {
            if (MyfeedBack == null) return null;

            IGeometry geometry = null;
            IPoint point = MyMapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
            if ((MyfeedBack as IMovePointFeedback) != null)
            {
                IMovePointFeedback pointMove = MyfeedBack as IMovePointFeedback;
                geometry = pointMove.Stop() as IGeometry;
            }
            else if ((MyfeedBack as ILineMovePointFeedback) != null)
            {
                ILineMovePointFeedback lineMove = MyfeedBack as ILineMovePointFeedback;
                geometry = lineMove.Stop() as IGeometry;
            }
            else if ((MyfeedBack as IPolygonMovePointFeedback) != null)
            {
                IPolygonMovePointFeedback polyMove = MyfeedBack as IPolygonMovePointFeedback;
                geometry = polyMove.Stop() as IGeometry;
            }
            MyfeedBack = null;
            MyMapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, MyselectedLayer, null);
            return geometry;
        }
        #endregion

        #region 移动要素

        public void FeatureMoveMouseDown(int x, int y)
        {
            if (MyselectedLayer == null) return;
            if ((MyselectedLayer as IGeoFeatureLayer) == null) return;

            IFeatureLayer featureLayer = MyselectedLayer as IFeatureLayer;
            IFeatureClass featureClass = featureLayer.FeatureClass;
            if (featureClass == null) return;

            if (MyselectedFeature.Count < 1) return;
            IFeature feature = MyselectedFeature[0];
            IGeometry startGeom = feature.Shape;

            IPoint point = MyMapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
            try
            {

                switch (featureClass.ShapeType)
                {
                    case esriGeometryType.esriGeometryPoint:
                        MyfeedBack = new MovePointFeedbackClass();
                        MyfeedBack.Display = MyMapControl.ActiveView.ScreenDisplay;
                        IMovePointFeedback pointMoveFeedback = MyfeedBack as IMovePointFeedback;
                        pointMoveFeedback.Start(startGeom as IPoint, point);
                        break;
                    case esriGeometryType.esriGeometryPolyline:
                        MyfeedBack = new MoveLineFeedbackClass();
                        MyfeedBack.Display = MyMapControl.ActiveView.ScreenDisplay;
                        IMoveLineFeedback lineMoveFeedback = MyfeedBack as IMoveLineFeedback;
                        lineMoveFeedback.Start(startGeom as IPolyline, point);
                        break;
                    case esriGeometryType.esriGeometryPolygon:
                        MyfeedBack = new MovePolygonFeedbackClass();
                        MyfeedBack.Display = MyMapControl.ActiveView.ScreenDisplay;
                        IMovePolygonFeedback polygonMoveFeedback = MyfeedBack as IMovePolygonFeedback;
                        polygonMoveFeedback.Start(startGeom as IPolygon, point);
                        break;
                    default:
                        break;
                }
            }
            catch { return; }
        }

        public void FeatureMoveMouseMove(int x, int y)
        {
            if (MyfeedBack == null) return;
            IPoint point = MyMapControl.ActiveView.ScreenDisplay.DisplayTransformation.ToMapPoint(x, y);
            MyfeedBack.MoveTo(point);
        }

        public IGeometry EndFeatureMove(int x, int y)
        {
            if (MyfeedBack == null) return null;
            IGeometry geometry = null;
            try
            {
                if ((MyfeedBack as IMovePointFeedback) != null)
                {
                    IMovePointFeedback pointMoveFeedback = MyfeedBack as IMovePointFeedback;
                    geometry = pointMoveFeedback.Stop();
                }
                else if ((MyfeedBack as IMoveLineFeedback) != null)
                {
                    IMoveLineFeedback lineMoveFeedback = MyfeedBack as IMoveLineFeedback;
                    geometry = lineMoveFeedback.Stop();
                }
                else if ((MyfeedBack as IMovePolygonFeedback) != null)
                {
                    IMovePolygonFeedback polygonMoveFeedback = MyfeedBack as IMovePolygonFeedback;
                    geometry = polygonMoveFeedback.Stop();
                }
            }
            catch { return null; }
            MyfeedBack = null;
            return geometry;
        }

        #endregion

        #region 更新要素(编辑、移动后)

        public bool UpdateEdit(IGeometry newGeom)
        {
            if (MyselectedFeature.Count < 1) return false;
            if (newGeom == null) return false;
            if (newGeom.IsEmpty) return false;

            IFeature feature = MyselectedFeature[0];
            IDataset dataset = feature.Class as IDataset;
            IWorkspaceEdit workspaceEdit = dataset.Workspace as IWorkspaceEdit;
            if (!workspaceEdit.IsBeingEdited()) return false;

            workspaceEdit.StartEditOperation();
            feature.Shape = newGeom;
            feature.Store();
            workspaceEdit.StopEditOperation();
            MyselectedFeature.Clear();
            MyselectedFeature.Add(feature);
            ClearSelection();
            //mMyMapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphicSelection, null, null);
            //mMyMapControl.Map.SelectFeature(mMyselectedLayer, feature);
            //mMyMapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphicSelection, null, null);
            return true;
        }

        public void UndoEdit()
        {
            if (MyselectedLayer == null) return;

            IFeatureLayer featLayer = MyselectedLayer as IFeatureLayer;
            IDataset dataset = featLayer.FeatureClass as IDataset;
            IWorkspaceEdit workspaceEdit = dataset.Workspace as IWorkspaceEdit;
            bool bHasUndos = false;
            workspaceEdit.HasUndos(ref bHasUndos);
            if (bHasUndos)
            {
                workspaceEdit.UndoEditOperation();
            }
            ClearSelection();
        }

        public void RedoEdit()
        {
            if (MyselectedLayer == null) return;

            IFeatureLayer featLayer = MyselectedLayer as IFeatureLayer;
            IDataset dataset = featLayer.FeatureClass as IDataset;
            IWorkspaceEdit workspaceEdit = dataset.Workspace as IWorkspaceEdit;
            bool bHasUndos = false;
            workspaceEdit.HasRedos(ref bHasUndos);
            if (bHasUndos)
            {
                workspaceEdit.RedoEditOperation();
            }
            ClearSelection();
        }

        #endregion

        #region 私有函数区
        private double ConvertPixelsToMapUnits(double pixelUnits)
        {
            int pixelExtent = MyMapControl.ActiveView.ScreenDisplay.DisplayTransformation.get_DeviceFrame().right
                           - MyMapControl.ActiveView.ScreenDisplay.DisplayTransformation.get_DeviceFrame().left;

            double realWorldDisplayExtent = MyMapControl.ActiveView.ScreenDisplay.DisplayTransformation.VisibleBounds.Width;
            double sizeOfOnePixel = realWorldDisplayExtent / pixelExtent;

            return pixelUnits * sizeOfOnePixel;
        }

        private bool TestGeometryHit(double tol, IPoint pPoint, IGeometry geometry, ref IPoint pHitPoint,
                                   ref double hitDist, ref int partIndex, ref int vertexOffset,
                                   ref int vertexIndex, ref bool vertexHit)
        {
            IHitTest pHitTest = geometry as IHitTest;
            pHitPoint = new PointClass();
            bool last = true;
            bool res = false;
            if (pHitTest.HitTest(pPoint, tol, esriGeometryHitPartType.esriGeometryPartVertex, pHitPoint, ref hitDist, ref partIndex, ref vertexIndex, ref last))
            {
                vertexHit = true;
                res = true;
            }
            else
            {
                if (pHitTest.HitTest(pPoint, tol, esriGeometryHitPartType.esriGeometryPartBoundary, pHitPoint, ref hitDist, ref partIndex, ref vertexIndex, ref last))
                {
                    vertexHit = false;
                    res = true;
                }
            }

            if (partIndex > 0)
            {
                IGeometryCollection pGeoColl = geometry as IGeometryCollection;
                vertexOffset = 0;
                for (int i = 0; i < partIndex; i = 2 * i + 1)
                {
                    IPointCollection pointColl = pGeoColl.get_Geometry(i) as IPointCollection;
                    vertexOffset = vertexOffset + pointColl.PointCount;
                }
            }
            return res;
        }

        #endregion

    }
}
