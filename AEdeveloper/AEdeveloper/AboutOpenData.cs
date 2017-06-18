using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Carto;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.DataSourcesFile;
using ESRI.ArcGIS.DataSourcesRaster;
using ESRI.ArcGIS.DataSourcesGDB;

namespace AEdeveloper
{
    class AboutOpenData
    {
        //添加各种文件
        //filetag取值
        //filetag=1，添加矢量数据
        //filetag=2，添加栅格数据
        //filetag=3，添加图层数据
        //filetag=4，添加个人数据库
        //filetag=5，添加CAD文件
        //filetag=6，添加地图文档
        public void Add_Any_layer(string DialogName,string FileClass,int filetag,AxMapControl mapControl)
        {
            for (int i = 0; i < mapControl.Map.LayerCount; i++)
            { mapControl.Map.get_Layer(i).Visible = false; }

            System.Windows.Forms.OpenFileDialog openfiledialog;
            openfiledialog = new OpenFileDialog();
            string dialogname = DialogName;// "打开矢量数据";
            string fileclass = FileClass;// "ShapeFile|*.shp";
            openfiledialog.Title = dialogname;
            openfiledialog.Filter = fileclass;
            try
            {
                if (openfiledialog.ShowDialog() == DialogResult.OK)
                {
                    string fullfilepath = openfiledialog.FileName;//存储打开文件的全路径
                    int index = fullfilepath.LastIndexOf("\\");
                    string filepath = fullfilepath.Substring(0, index); //获得文件路径
                    string filename = fullfilepath.Substring(index + 1);//获得文件名称 
                    switch (filetag)
                    {
                        case 1: addshapefile(filepath, filename, mapControl); break;//添加矢量数据函数
                        case 2: addrasterlayer(filepath, filename, mapControl); break;//添加栅格数据函数
                        case 3: addLyrLayer(fullfilepath, mapControl); break;//添加lyr图层数据
                        case 4: addGeodatabaseLayer(fullfilepath, mapControl); break;//添加个人数据库(person Geodatabase)
                        case 5: addcadfile(filepath, filename, mapControl); break;//添加cad文件
                        case 6: addmxdfile(fullfilepath, mapControl); break;//加载Mxd文件函数 
                        default:break;
                    }          
                }
            }
            catch (Exception ee)
            {
               MessageBox.Show("添加图层失败" + ee.ToString());
            }
        }

        //是否保存当前地图
        public void IfSaveTheCurrentMap(AxMapControl axMapControl1)
        {
            if (axMapControl1.LayerCount > 0)
            {
                DialogResult result = MessageBox.Show("是否保存当前地图？", "警告", MessageBoxButtons.YesNoCancel, MessageBoxIcon.Question);
                if (result == DialogResult.Cancel) return;
                if (result == DialogResult.Yes)
                {
                    if (null != axMapControl1.DocumentFilename && axMapControl1.CheckMxFile(axMapControl1.DocumentFilename))
                    {
                        // 创建一个新的地图文档实例 
                        IMapDocument mapDoc = new MapDocumentClass();
                        // 打开当前地图文档 
                        mapDoc.Open(axMapControl1.DocumentFilename, string.Empty);
                        // 保存地图文档 
                        mapDoc.Save(mapDoc.UsesRelativePaths, false);
                        mapDoc.Close();
                    }
                }
            }
        }

        //加载Mxd文件函数
        private void addmxdfile(string fullfilepath, AxMapControl axMapControl1)
        {
            if (fullfilepath == "") return;

            if (axMapControl1.CheckMxFile(fullfilepath))
            {
                axMapControl1.MousePointer = esriControlsMousePointer.esriPointerHourglass;
                axMapControl1.LoadMxFile(fullfilepath, 0, Type.Missing);
                IActiveView activeViw = axMapControl1.Map as IActiveView;
                activeViw.Extent = axMapControl1.FullExtent;
                axMapControl1.Refresh();
                axMapControl1.MousePointer = esriControlsMousePointer.esriPointerDefault;
            }
            else if (fullfilepath != "")
            {
                MessageBox.Show(fullfilepath + "是无效的地图文档");
            }
        }

        //添加矢量数据函数
        private void addshapefile(string filepath, string filename, AxMapControl mapControl)
        {
            //1.构建工作空间工厂
            IWorkspaceFactory pWF = new ShapefileWorkspaceFactory();
            //2.利用工厂建立工作空间
            IFeatureWorkspace pFW = pWF.OpenFromFile(filepath, 0) as IFeatureWorkspace;
            //3.使用工作区打开并得到FeatureClass 
            IFeatureClass featureClass = pFW.OpenFeatureClass(filename);
            //4.把FeatureClass装载到新建的图层实例
            IFeatureLayer featurelay = new FeatureLayer(); featurelay.FeatureClass = featureClass;
            //5.把图层加载到MapControl控件**//MapControl.ClearLayers();//清空原有图层
            mapControl.Map.AddLayer(featurelay);//or//this.MapControl.Map.AddShapeFile(filepath, filename);//添加图层
            IActiveView activeViw = mapControl.Map as IActiveView;
            activeViw.Extent = mapControl.FullExtent;
            //6.刷新mapcontrol
            mapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, featurelay, null);
        }

        //添加栅格数据函数
        private void addrasterlayer(string filepath, string filename, AxMapControl mapControl)
        {
            //1.构建工作空间工厂
            IWorkspaceFactory pWF = new RasterWorkspaceFactory();
            //2.利用工厂建立工作空间
            IRasterWorkspace pRW = pWF.OpenFromFile(filepath, 0) as IRasterWorkspace;
            //3.使用工作区打开并得到RasterDataset
            IRasterDataset pRDataset = pRW.OpenRasterDataset(filename);
            //4.把RasterDataset装载到新建的图层实例
            IRasterLayer rasterLay = new RasterLayer(); rasterLay.CreateFromDataset(pRDataset);
            //5.把图层加载到MapControl控件**//MapControl.ClearLayers();//清空原有图层
            mapControl.Map.AddLayer(rasterLay);//or//this.MapControl.Map.AddShapeFile(filepath, filename);//添加图层
            IActiveView activeViw = mapControl.Map as IActiveView;
            activeViw.Extent = mapControl.FullExtent;
            //6.刷新mapcontrol
            mapControl.ActiveView.PartialRefresh(esriViewDrawPhase.esriViewGeography, rasterLay, null);
        }

        //添加lyr图层数据
        private void addLyrLayer(string fullfilepath, AxMapControl mapControl)
        {
            if (fullfilepath == "") return;
            mapControl.AddLayerFromFile(fullfilepath, 0);//添加lyr图层
            IActiveView activeView = mapControl.Map as IActiveView;
            activeView.Extent = mapControl.FullExtent;
            mapControl.Refresh();
        }

        //添加个人数据库(person Geodatabase)
        public void addGeodatabaseLayer(string strFullPath, AxMapControl mapControl)
        {
            if (strFullPath == "") return;
            //1.构建工作空间工厂
            IWorkspaceFactory pAWF = new AccessWorkspaceFactoryClass();
            IFeatureWorkspace pFW; IFeatureLayer pFL; IFeatureDataset pFD;
            //2.使用工作区打开person Geodatabase,并添加图层   
            IWorkspace pWorkspace = pAWF.OpenFromFile(strFullPath, 0);
            //打开工作空间并遍历数据集
            IEnumDataset pEnumDataset = pWorkspace.get_Datasets(ESRI.ArcGIS.Geodatabase.esriDatasetType.esriDTAny);
            pEnumDataset.Reset();
            IDataset pDataset = pEnumDataset.Next();
            //如果数据集是IFeatureDataset,则遍历它下面的子类
            if (pDataset is IFeatureDataset)
            {
                pFW = (IFeatureWorkspace)pAWF.OpenFromFile(strFullPath, 0);
                pFD = pFW.OpenFeatureDataset(pDataset.Name);
                IEnumDataset pEnumDataset1 = pFD.Subsets;
                pEnumDataset1.Reset();
                IDataset pDataset1 = pEnumDataset1.Next();
                //如果子类是FeatureClass，则添加到MapControl中
                if (pDataset1 is IFeatureClass)
                {
                    pFL = new FeatureLayerClass();
                    pFL.FeatureClass = pFW.OpenFeatureClass(pDataset1.Name);
                    pFL.Name = pFL.FeatureClass.AliasName;
                    mapControl.Map.AddLayer(pFL);
                    mapControl.ActiveView.Refresh();
                }
            }
        }
 
        //添加cad文件
        public void addcadfile(string filePath, string filename, AxMapControl mapControl)
        {
            if (filename + filePath == "") return;
            IWorkspaceFactory pWorkspaceFactory = new CadWorkspaceFactoryClass();//打开CAD数据集
            IFeatureWorkspace pFeatureWorkspace = (IFeatureWorkspace)pWorkspaceFactory.OpenFromFile(filePath, 0);//打开一个要素集
            IFeatureLayer pFeatureLayer;
            IFeatureDataset pFeatureDataset = pFeatureWorkspace.OpenFeatureDataset(filename);
            //IFeaturClassContainer可以管理IFeatureDataset中的每个要素类   
            IFeatureClassContainer pFeatClassContainer = (IFeatureClassContainer)pFeatureDataset;
            //对CAD文件中的要素进行遍历处理 
            for (int i = 0; i < pFeatClassContainer.ClassCount - 1; i++)
            {
                IFeatureClass pFeatClass = pFeatClassContainer.get_Class(i);
                if (pFeatClass.FeatureType == esriFeatureType.esriFTCoverageAnnotation)
                {
                    //如果是注记，则添加注记层
                    pFeatureLayer = new CadAnnotationLayerClass();
                }
                else
                {
                    //如果是点、线、面，则添加要素层
                    pFeatureLayer = new FeatureLayerClass();
                    pFeatureLayer.Name = pFeatClass.AliasName;
                    pFeatureLayer.FeatureClass = pFeatClass;
                    mapControl.Map.AddLayer(pFeatureLayer);
                    mapControl.ActiveView.Refresh();
                }
            }
        }
    }
}
