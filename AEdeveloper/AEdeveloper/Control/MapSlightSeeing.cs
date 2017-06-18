using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Data;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geometry;
using ESRI.ArcGIS.Controls;

namespace AEdeveloper.Control
{
    public partial class MapSlightSeeing : UserControl
    {
        MyMapContext _myfrm;
        public MapSlightSeeing(MyMapContext parent)
        {
            InitializeComponent();
            _myfrm = parent;
        }
        
        //3. 实现互动
        private void axMapControl1_OnMouseDown(object sender, ESRI.ArcGIS.Controls.IMapControlEvents2_OnMouseDownEvent e)
        {
            if (this.axMapControl1.Map.LayerCount > 0)
            {
                if (e.button == 1)
                {
                    IPoint pPt = new PointClass();
                    pPt.PutCoords(e.mapX, e.mapY);
                    //改变主控件的视图范围
                    _myfrm._mainFrm.axMapControl1.CenterAt(pPt);
                    _myfrm._mainFrm.axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);

                }
                else if (e.button == 2)
                {
                    IEnvelope pEnv = this.axMapControl1.TrackRectangle();
                    _myfrm._mainFrm.axMapControl1.Extent = pEnv; ;
                    _myfrm._mainFrm.axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
                }
            }
        }

        private void axMapControl1_OnMouseMove(object sender, IMapControlEvents2_OnMouseMoveEvent e)
        {
            if (e.button == 1)
            {
                IPoint pPt = new PointClass();
                pPt.PutCoords(e.mapX, e.mapY);
                _myfrm._mainFrm.axMapControl1.CenterAt(pPt);
                _myfrm._mainFrm.axMapControl1.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGraphics, null, null);
            }
        }
        //Full Extent
        private void button8_Click(object sender, EventArgs e)
        {
            this.axMapControl1.CurrentTool = null;
            this.axMapControl1.MousePointer = esriControlsMousePointer.esriPointerArrow;
            this.axMapControl1.Extent = this.axMapControl1.FullExtent;
        }
    }
}
