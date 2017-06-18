using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using ESRI.ArcGIS.Controls;
using ESRI.ArcGIS.Geodatabase;
using ESRI.ArcGIS.Carto;
using System.Collections;

namespace AEdeveloper
{
    public partial class SearchByAttribute : Form
    {
        private AxMapControl pAxMapControl;
        public SearchByAttribute(AxMapControl lAxMapControl)
        {
            InitializeComponent();
            this.pAxMapControl = lAxMapControl;
            //将所有图层名添加进comboBoxLayer，并设置默认值为第一项
            for (int i = 0; i < pAxMapControl.LayerCount; i++)
            {
                LayerComBOX.Items.Add(pAxMapControl.get_Layer(i).Name);
            }
            LayerComBOX.SelectedIndex = 0;
            //设置comboBoxMethod的选择项，并设置默认值为第一项
            TheChooseMean.Items.Add("创建新选择内容");
            TheChooseMean.Items.Add("添加到当前选择内容");
            TheChooseMean.Items.Add("从当前选择内容中移除");
            TheChooseMean.Items.Add("从当前选择内容中选择");
            TheChooseMean.SelectedIndex = 3;
            weiyizhi.Visible = false;
        }

        private void dengyu_Click(object sender, EventArgs e)
        {
            selectwords.Text += " =";
        }

        private void budengyu_Click(object sender, EventArgs e)
        {
            selectwords.Text += " <>";
        }

        private void dayu_Click(object sender, EventArgs e)
        {
            selectwords.Text += " >";
        }

        private void dayudengyu_Click(object sender, EventArgs e)
        {
            selectwords.Text += " >=";
        }

        private void xiaoyu_Click(object sender, EventArgs e)
        {
            selectwords.Text += " <";
        }

        private void xiaoyudengyu_Click(object sender, EventArgs e)
        {
            selectwords.Text += " <=";
        }

        private void like_Click(object sender, EventArgs e)
        {
            selectwords.Text += " LIKE";
        }

        private void and_Click(object sender, EventArgs e)
        {
            selectwords.Text += " AND";
        }

        private void or_Click(object sender, EventArgs e)
        {
            selectwords.Text += " OR";
        }

        private void not_Click(object sender, EventArgs e)
        {
            selectwords.Text += " NOT";
        }

        private void itis_Click(object sender, EventArgs e)
        {
            selectwords.Text += " IS";
        }

        private void baokuo_Click(object sender, EventArgs e)
        {
            selectwords.Text += " ()";
        }

        private void jian_Click(object sender, EventArgs e)
        {
            selectwords.Text += " -";
        }

        private void beifenzhi_Click(object sender, EventArgs e)
        {
            selectwords.Text += " %";
        }

        private void Clearit_Click(object sender, EventArgs e)
        {
            selectwords.Text = "";
        }

        //双击选中字段，将其添加进条件框
        //当选择另一个字段时，将List_Value框隐藏
        //应用按钮

        private void getuniquevalue()
        {
            //获取要素图层与要素类，将其作为参数传入GetUniqueValue()函数
            IFeatureLayer lFeatureLayer = (IFeatureLayer)pAxMapControl.get_Layer(LayerComBOX.SelectedIndex);
            IFeatureClass lFeatureClass = lFeatureLayer.FeatureClass;
            //将返回的所有值存入allValue数组中,并进行排序
            string[] allValue = GetUniqueValue(lFeatureClass, SelectedfieldList.Text);
            System.Array.Sort(allValue);
            //获取字段对象，用于在将其值添加进listbox_value中时判断字段类型
            IFeatureCursor lFeatureCursor = lFeatureLayer.Search(null, true);
            IFeature lFeature = lFeatureCursor.NextFeature();
            IField lField = new FieldClass();
            for (int j = 0; j < lFeature.Fields.FieldCount; j++)
            {
                if (SelectedfieldList.Text == lFeature.Fields.get_Field(j).Name)
                {
                    lField = lFeature.Fields.get_Field(j);
                }
            }
            //将之前listBox_Value中的值清空，然后添加此次选中字段的所有数据
            weiyizhi.Items.Clear();
            for (int i = 0; i < allValue.Length; i++)
            {
                if (lField.Type == esriFieldType.esriFieldTypeString)
                {
                    allValue[i] = "'" + allValue[i] + "'";
                    weiyizhi.Items.Add(allValue[i]);
                }
                else
                {
                    weiyizhi.Items.Add(allValue[i]);
                }
            }
            weiyizhi.Visible = true;
            onlyvalue.Enabled = false;
        }

        public string[] GetUniqueValue(IFeatureClass pFeatureClass, string strFld)
        {
            //得到IFeatureCursor游标
            IFeatureCursor pCursor = pFeatureClass.Search(null, false);
            //IDataStatistics对象实例生成
            IDataStatistics pData = new DataStatisticsClass();
            pData.Field = strFld;
            pData.Cursor = pCursor as ICursor;
            //枚举唯一值
            IEnumerator pEnumVar = pData.UniqueValues;
            //记录总数
            int RecordCount = pData.UniqueValueCount;
            //字符数组
            string[] strValue = new string[RecordCount];
            pEnumVar.Reset();
            int i = 0;
            while (pEnumVar.MoveNext())
            {
                strValue[i++] = pEnumVar.Current.ToString();
            }
            return strValue;
        }

        private void SELECTFeature()
        {
            if (selectwords.Text != "")
            {
                try
                {
                    IFeatureLayer lFeatureLayer = (IFeatureLayer)pAxMapControl.get_Layer(LayerComBOX.SelectedIndex);
                    IFeatureSelection lFeatureSelection = (IFeatureSelection)lFeatureLayer;
                    //判断选择的SQL方法的类型
                    esriSelectionResultEnum lesriSREnum = esriSelectionResultEnum.esriSelectionResultNew;
                    switch (weiyizhi.SelectedIndex)
                    {
                        case 0:
                            lesriSREnum = esriSelectionResultEnum.esriSelectionResultNew;
                            break;
                        case 1:
                            lesriSREnum = esriSelectionResultEnum.esriSelectionResultAdd;
                            break;
                        case 2:
                            lesriSREnum = esriSelectionResultEnum.esriSelectionResultSubtract;
                            break;
                        case 3:
                            lesriSREnum = esriSelectionResultEnum.esriSelectionResultAnd;
                            break;
                        default:
                            MessageBox.Show("请选择一种查询方法");
                            break;
                    }
                    //创建查询的条件
                    IQueryFilter2 lQueryFilter = new QueryFilterClass();
                    lQueryFilter.WhereClause = selectwords.Text;
                    //根据查询添加进行选择，并刷新屏幕
                    lFeatureSelection.SelectFeatures(lQueryFilter, lesriSREnum, false);
                    pAxMapControl.ActiveView.Refresh();
                }
                catch (Exception)
                {
                    MessageBox.Show("输入的SQL查询不符合语法要求");
                }
            }
            else
            {
                MessageBox.Show("请选择需要查询的图层");
            }
        }

        private void GETFIELD()
        {
            //如果没有图层被选择，将不能添加字段
            if (LayerComBOX.Text != null)
            {
                //清空listBox_Field
                SelectedfieldList.Items.Clear();
                //获取要素
                try
                {
                    IFeatureLayer lFeatureLayer = (IFeatureLayer)pAxMapControl.Map.get_Layer(LayerComBOX.SelectedIndex);
                    IFeatureCursor lFeatureCursor = lFeatureLayer.Search(null, true);
                    IFeature lFeature = lFeatureCursor.NextFeature();
                    //循环添加所属图层的字段名进入listBox_Field中
                    //对于esriFieldTypeGeometry类型的自动则不予以添加
                    for (int i = 0; i < lFeature.Fields.FieldCount; i++)
                    {
                        if (lFeature.Fields.get_Field(i).Type != esriFieldType.esriFieldTypeGeometry)
                        {
                            SelectedfieldList.Items.Add(lFeature.Fields.get_Field(i).Name);
                        }
                    }
                    //设置当前选择字段为第一个
                    SelectedfieldList.SelectedIndex = 0;
                }
                catch { MessageBox.Show("该图层不可用，请选择一个要素图层"); }
                //将描述信息修改
                SelectFrom.Text = "SELECT * FROM " + LayerComBOX.Text + " WHERE:";
            }
        }

        private void SelectedfieldList_SelectedIndexChanged(object sender, EventArgs e)
        {
            weiyizhi.Visible = false;
            onlyvalue.Enabled = true;
        }

        private void CloseIt_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void onlyvalue_Click(object sender, EventArgs e)
        {
            getuniquevalue();
        }

        private void weiyizhi_DoubleClick(object sender, EventArgs e)
        {
            selectwords.Text += " " + weiyizhi.Text;
        }

        private void SelectedfieldList_DoubleClick(object sender, EventArgs e)
        {
            IFeatureLayer lFeatureLayer = (IFeatureLayer)pAxMapControl.Map.get_Layer(LayerComBOX.SelectedIndex);
            if (lFeatureLayer.DataSourceType == "Shapefile Feature Class")//shapefile文件
            {
                selectwords.Text += " " + SelectedfieldList.Text + " ";
            }
            else
            {
                selectwords.Text += " " + SelectedfieldList.Text + " ";
            }
        }

        private void LayerComBOX_SelectedIndexChanged(object sender, EventArgs e)
        {
            GETFIELD();
        }

        private void UsingIt_Click(object sender, EventArgs e)
        {
            SELECTFeature();
        }

        private void Addit_Click(object sender, EventArgs e)
        {
            GETFIELD();
        }

        private void SearchIt_Click(object sender, EventArgs e)
        {
            GETFIELD();
            this.Close();
        }

        private void DisplayShpLayer_CheckedChanged(object sender, EventArgs e)
        {
            LayerComBOX.Items.Clear();
            if (DisplayShpLayer.Checked == true)
            {
                IFeatureLayer pfeaturelayer = null;
                for (int i = 0; i < pAxMapControl.LayerCount; i++)
                {
                    pfeaturelayer = pAxMapControl.get_Layer(i) as IFeatureLayer;
                    if (pfeaturelayer == null)
                        continue;
                    else
                        LayerComBOX.Items.Add(pAxMapControl.get_Layer(i).Name);
                }
            }
            else
            {
                for (int i = 0; i < pAxMapControl.LayerCount; i++)
                {
                    LayerComBOX.Items.Add(pAxMapControl.get_Layer(i).Name);
                }
            }
        }


    }
}
