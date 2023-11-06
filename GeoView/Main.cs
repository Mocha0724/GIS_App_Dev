﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.IO;
using Npgsql;
using GeoView.DataIOTools;
using MyMapObjects;

namespace GeoView
{
    public partial class Main : Form
    {
        #region 字段

        //(1)选项变量
        private Color mZoomBoxColor = Color.DeepPink;   //缩放盒颜色
        private double mZoomBoxWidth = 0.53;    //缩放盒边界宽度，单位毫米
        private Color mSelectBoxColor = Color.DarkGreen;    //选择盒颜色
        private double mSelectBoxWidth = 0.53;  //选择盒边界宽度
        private double mZoomRatioFixed = 2; //固定放大系数
        private double mZoomRatioMouseWheel = 1.2;  //滚轮放大系数
        private double mSelectingTolerance = 3; //选择容限，单位为像素
        private MyMapObjects.moSimpleFillSymbol mSelectBoxSymbol;    //选择盒符号
        private MyMapObjects.moSimpleFillSymbol mZoomBoxSymbol;    //缩放盒符号
        private MyMapObjects.moSimpleFillSymbol mMovingPolygonSymbol;   //正在移动的多边形符号
        private MyMapObjects.moSimpleLineSymbol mMovingPolylineSymbol;
        private MyMapObjects.moSimpleMarkerSymbol mMovingPointSymbol;
        private MyMapObjects.moSimpleFillSymbol mEditingPolygonSymbol;  //正在编辑的多边形符号
        private MyMapObjects.moSimpleLineSymbol mEditingPolylineSymbol;
        private MyMapObjects.moSimpleMarkerSymbol mEditingVertexSymbol; //正在编辑的图形顶点的符号
        private MyMapObjects.moSimpleLineSymbol mElasticSymbol; //橡皮筋符号
        private bool mShowLngLat = false;   //是否显示经纬度
        private bool mEditMoMap = false;
        public List<AttributeTable> All_tables = new List<AttributeTable>();//所有属性表的集合
        public int Fid_table_windows = 0;//给属性表们进行一个唯一赋值
        //(2)与地图操作有关的变量
        private Int32 mMapOpStyle = 0;  //0：无，1：编辑（可选择可移动）,2:描绘要素；3.编辑节点；4.漫游；5.放大；6.缩小；7.选择；8.识别
        private Int32 mOperatingLayerIndex  //当前操作的图层的索引
        {
            get { return GetOpLayerIndex(); }
        }
        private PointF mStartMouseLocation;
        private bool mIsInZoomIn = false;
        private bool mIsInPan = false;
        private bool mIsInSelect = false;
        private bool mIsInMove = false;
        private bool mSelectedIsMoved = false;  //选择的图形是否被移动
        private bool mIsInIdentify = false;
        private bool mIsInRenderer = false;
        private bool mIsInEditPoint = false;    //编辑节点状态
        private bool mNeedToSave = false;   //是否需要保存数据
        private bool mPointEditNeedSave = false;//对节点的编辑是否被保存
        public Int32 mLastOpLayerIndex = -1;   //最近一次操作的图层索引
        private Int32 mMouseOnPartIndex = -1;   //鼠标位于多边形部件的索引
        private Int32 mMouseOnPointIndex = -1;  //鼠标位于多边形部件顶点的索引
        private List<Int32> mLayerIndex = new List<Int32>();
        //撤销节点编辑所需变量
        private List<Int32> mEditPointOperation = new List<Int32>();
        private List<MyMapObjects.moPoint> mEditPointRecord = new List<MyMapObjects.moPoint>();
        private List<Int32> mPastPartIndex = new List<Int32>();
        private List<Int32> mPastPointIndex = new List<Int32>();
        private int index_dbf = 0;
        private int index_gvshp = 0;

        private bool mIsInMovePoint
        {
            get { return MovePointBtn.Checked; }
        }
        private bool mIsInAddPoint
        {
            get { return AddPointBtn.Checked; }
        }
        private bool mIsInDeletePoint
        {
            get { return DeletePointBtn.Checked; }
        }

        private List<MyMapObjects.moGeometry> mMovingGeometries = new List<MyMapObjects.moGeometry>();  //正在移动的图形的集合
        private MyMapObjects.moGeometry mEditingGeometry;   //正在编辑的图形
        private List<MyMapObjects.moPoints> mSketchingShape;    //正在描绘的图形，用一个多点集合存储；
        private List<MyMapObjects.moPoint> mSketchingPoint; //正在描绘的点
        private List<MyMapObjects.moGeometry> mCopyingGeometries = new List<MyMapObjects.moGeometry>();
        private MyMapObjects.moGeometryTypeConstant mCopyingType;


        //(3)与文件操作相关的变量
        public List<DataIOTools.gvShpFileManager> mGvShapeFiles = new List<DataIOTools.gvShpFileManager>();    //管理要素文件
        public List<DataIOTools.dbfFileManager> mDbfFiles = new List<DataIOTools.dbfFileManager>();    //管理属性文件

        //(4)与图层渲染有关的变量
        private Int32 mPointRendererMode = 0; //渲染方式,0:简单渲染,1:唯一值渲染,2:分级渲染
        private Int32 mPointSymbolStyle = 0; //样式索引
        private Color mPointSimpleRendererColor = Color.Red; //符号颜色
        private Double mPointSimpleRendererSize = 5; //符号尺寸
        private Int32 mPointUniqueFieldIndex = 0; //绑定字段索引
        private Double mPointUniqueRendererSize = 5; //符号尺寸
        private Int32 mPointClassBreaksFieldIndex = 0; //绑定字段索引
        private Int32 mPointClassBreaksNum = 5; //分类数
        private Color mPointClassBreaksRendererColor = Color.Red; //符号颜色
        private Double mPointClassBreaksRendererMinSize = 3; //符号起始尺寸,点图层采用符号尺寸进行分级表示
        private Double mPointClassBreaksRendererMaxSize = 6; //符号终止尺寸

        private Int32 mPolylineRendererMode = 0; //渲染方式,0:简单渲染,1:唯一值渲染,2:分级渲染
        private Int32 mPolylineSymbolStyle = 0; //样式索引
        private Color mPolylineSimpleRendererColor = Color.Red; //符号颜色
        private Double mPolylineSimpleRendererSize = 0.5; //符号尺寸
        private Int32 mPolylineUniqueFieldIndex = 0; //绑定字段索引
        private Double mPolylineUniqueRendererSize = 0.5; //符号尺寸
        private Int32 mPolylineClassBreaksFieldIndex = 0; //绑定字段索引
        private Int32 mPolylineClassBreaksNum = 5; //分类数
        private Color mPolylineClassBreaksRendererColor = Color.Red; //符号颜色
        private Double mPolylineClassBreaksRendererMinSize = 0.5; //符号起始尺寸,线图层采用符号尺寸进行分级表示
        private Double mPolylineClassBreaksRendererMaxSize = 1.5; //符号终止尺寸

        private Int32 mPolygonRendererMode = 0; //渲染方式,0:简单渲染,1:唯一值渲染,2:分级渲染
        private Color mPolygonSimpleRendererColor = Color.Red; //符号颜色
        private Int32 mPolygonUniqueFieldIndex = 0; //绑定字段索引
        private Int32 mPolygonClassBreaksFieldIndex = 0; //绑定字段索引
        private Int32 mPolygonClassBreaksNum = 5; //分类数
        private Color mPolygonClassBreaksRendererStartColor = Color.MistyRose; //符号起始颜色,面图层采用符号颜色进行分级表示
        private Color mPolygonClassBreaksRendererEndColor = Color.Red; //符号终止颜色
        //(5)与显示注记有关变量
        private Color mLabelColor = Color.Black;
        private Font mLabelFont = new Font("宋体", 12);
        private Int32 mLabelFieldIndex = 0;
        private bool mLabelUseMask = false;
        private bool mLabelVisible = false;

        PointRenderer mPointRenderer;
        PolylineRenderer mPolylineRenderer;
        PolygonRenderer mPolygonRenderer;
        Label mLabel;

        #endregion

        public Main()
        {
            InitializeComponent();
            this.MouseWheel += moMap_MouseWheel;
            GvToolsInitial();
        }

        private void Main_Load(object sender, EventArgs e)
        {
            //(1)初始化符号
            InitializeSymbols();
            //(2)初始化描绘图形
            InitializeSketchingShape();
            //(3)显示比例尺
            ShowMapScale();
        }
        public void RedrawAttribute()
        {
            for (int i = 0; i < All_tables.Count; i++)
                All_tables[i].refresh();//将每个列表进行一个无脑刷新
        }
        #region 控件

        //保存图片
        private void btnSaveProject_Click(object sender, EventArgs e)
        {
            if (moMap.Layers.Count == 0) MessageBox.Show("请先导入图层！");
            else
            {
                SaveFileDialog save_file = new SaveFileDialog();
                save_file.OverwritePrompt = true;
                save_file.RestoreDirectory = true;
                save_file.Title = "保存bmp图片";
                save_file.Filter = "BMP文件(*.bmp)|*.bmp";
                if (save_file.ShowDialog() == DialogResult.OK)
                {
                    string file_name = save_file.FileName;
                    moMap.BmpMap.Save(file_name, System.Drawing.Imaging.ImageFormat.Bmp);
                }
            }
        }


        //编辑
        private void EditSpBtn_Click(object sender, EventArgs e)
        {
            if (moMap.Layers.Count > 0)
            {
                BeginEditItem.Enabled = true;
            }
        }

        //开始编辑
        private void BeginEditItem_Click(object sender, EventArgs e)
        {
            EndEditItem.Enabled = true;
            SaveEditItem.Enabled = true;
            MoveFeatureBtn.Enabled = true;
            EditPointBtn.Enabled = true;
            CreateFeatureBtn.Enabled = true;
            SelectLayer.Enabled = true;
            RefreshSelectLayer();
            MoveFeatureBtn_Click(sender, e);
            mNeedToSave = false;
            mEditMoMap = true;
        }

        //结束编辑
        private void EndEditItem_Click(object sender, EventArgs e)
        {
            if (mPointEditNeedSave)
            {
                SavePointEdit();
            }
            if (mNeedToSave)
            {
                DialogResult dr = MessageBox.Show("是否保存编辑内容", "Saving", MessageBoxButtons.YesNoCancel);
                if (dr == DialogResult.Cancel)
                {
                    return;
                }
                else if (dr == DialogResult.Yes)
                {
                    SaveEditItem_Click(sender, e);
                }
                else
                {
                    CancelEdit();
                }
            }
            MoveFeatureBtn.Enabled = false;
            EditPointBtn.Enabled = false;
            CreateFeatureBtn.Enabled = false;
            MoveFeatureBtn.Checked = false;
            EditPointBtn.Checked = false;
            CreateFeatureBtn.Checked = false;
            SelectLayer.SelectedIndex = -1;
            SelectLayer.Enabled = false;
            EndEditItem.Enabled = false;
            SaveEditItem.Enabled = false;
            mNeedToSave = false;
            mMapOpStyle = 0;
            for (Int32 i = 0; i < moMap.Layers.Count; i++)
            {
                moMap.Layers.GetItem(i).SelectedFeatures.Clear();
                moMap.Layers.GetItem(i).SelectIndex.Clear();
            }
            mMovingGeometries.Clear();
            moMapRightMenu.Items.Clear();
            InitializeSketchingShape();
            mEditingGeometry = null;
            mEditMoMap = false;
            moMap.RedrawMap();
        }

        //保存编辑
        private void SaveEditItem_Click(object sender, EventArgs e)
        {
            try
            {
                if (mPointEditNeedSave)
                {
                    SavePointEdit();
                }
                if (mNeedToSave)
                {
                    mNeedToSave = false;
                    SaveMapLayer(mOperatingLayerIndex);
                }
            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString());
                return;
            }
        }

        //清空选择
        private void ClearSelectionBtn_Click(object sender, EventArgs e)
        {
            for (Int32 i = 0; i < moMap.Layers.Count; i++)
            {
                moMap.Layers.GetItem(i).SelectedFeatures.Clear();
                moMap.RedrawMap();
            }
        }

        //编辑要素
        private void MoveFeatureBtn_Click(object sender, EventArgs e)
        {
            if (mOperatingLayerIndex == -1) return;
            SavePointEdit();
            MoveFeatureBtn.Checked = true;
            EditPointBtn.Checked = false;
            CreateFeatureBtn.Checked = false;
            mMapOpStyle = 1;    //默认为编辑操作
            RightMenuInSelect();
        }

        //编辑折点
        private void EditPointBtn_Click(object sender, EventArgs e)
        {
            if (EditPointBtn.Checked)
            {
                MoveFeatureBtn_Click(sender, e);
            }
            else
            {
                if (mOperatingLayerIndex == -1) return;
                MyMapObjects.moMapLayer sLayer = moMap.Layers.GetItem(mOperatingLayerIndex);
                if (sLayer.SelectedFeatures.Count != 1)
                {
                    MessageBox.Show("请选择且仅选择一个可编辑的要素进行修改！");
                    return;
                }
                MoveFeatureBtn.Checked = false;
                EditPointBtn.Checked = true;
                CreateFeatureBtn.Checked = false;
                mMapOpStyle = 3;
                RightMenuInEdit();
                ShowEditStrip(sLayer.ShapeType);
                ShowEditGeometry();
            }
        }
        private void EditPointBtn_CheckedChanged(object sender, EventArgs e)
        {
            if (EditPointBtn.Checked == false)
            {
                HideEditStrip();
            }
        }

        //新建要素
        private void CreateFeatureBtn_Click(object sender, EventArgs e)
        {
            if (mOperatingLayerIndex == -1) return;
            SavePointEdit();
            MoveFeatureBtn.Checked = false;
            EditPointBtn.Checked = false;
            CreateFeatureBtn.Checked = true;
            mMapOpStyle = 2;
            RightMenuInSketch();
        }

        //移动节点
        private void MovePointBtn_Click(object sender, EventArgs e)
        {
            MovePointBtn.Checked = true;
            AddPointBtn.Checked = false;
            DeletePointBtn.Checked = false;
        }

        //增加节点
        private void AddPointBtn_Click(object sender, EventArgs e)
        {
            MovePointBtn.Checked = false;
            AddPointBtn.Checked = true;
            DeletePointBtn.Checked = false;
        }

        //删除节点
        private void DeletePointBtn_Click(object sender, EventArgs e)
        {
            MovePointBtn.Checked = false;
            AddPointBtn.Checked = false;
            DeletePointBtn.Checked = true;
        }

        //选择操作图层
        private void SelectLayer_SelectedIndexChanged(object sender, EventArgs e)
        {
            bool needSave = false;
            if (mNeedToSave)
            {
                EndEditItem_Click(sender, e);
                needSave = true;
            }
            if (SelectLayer.SelectedIndex == -1)
            {
                SelectLayer.DropDownStyle = ComboBoxStyle.DropDown;
                SelectLayer.Text = "请选择图层";
            }
            else
            {
                SelectLayer.DropDownStyle = ComboBoxStyle.DropDownList;
            }
            if (mOperatingLayerIndex != -1 && mGvShapeFiles[mOperatingLayerIndex].SourceFileType == "shp")
            {
                Int32 goOn = (Int32)MessageBox.Show("当前图层为shapefile文件，对其进行编辑操作，会在同一目录下生成同名gvshp文件，操作只会保存在该gvshp文件中！若已有同名gvshp文件，会进行覆盖，请知悉！是否继续？",
                    "Warning", MessageBoxButtons.OKCancel, MessageBoxIcon.Exclamation);

                if (goOn == 1)
                {
                    mGvShapeFiles[mOperatingLayerIndex].SourceFileType = "gvshp";
                    string filePath = mGvShapeFiles[mOperatingLayerIndex].DefaultFilePath;
                    string sPath = filePath.Substring(0, filePath.IndexOf(".shp")) + ".gvshp";
                    mGvShapeFiles[mOperatingLayerIndex].SaveToFile(sPath);
                    mGvShapeFiles[mOperatingLayerIndex].DefaultFilePath = sPath;
                    filePath = mDbfFiles[mOperatingLayerIndex].DefaultPath;
                    sPath = filePath.Substring(0, filePath.IndexOf(".dbf")) + ".gvdbf";
                    mDbfFiles[mOperatingLayerIndex].SaveToFile(sPath);
                    mDbfFiles[mOperatingLayerIndex].DefaultPath = sPath;
                }
                else
                {
                    Int32 temp = mLastOpLayerIndex;
                    mLastOpLayerIndex = -1;
                    SelectLayer.SelectedIndex = -1;
                    mLastOpLayerIndex = temp;
                }
            }
            if (needSave && mOperatingLayerIndex != -1)
            {
                BeginEditItem_Click(sender, e);
            }
        }

        //漫游
        private void btnPan_Click(object sender, EventArgs e)
        {
            mMapOpStyle = 4;
        }

        //全范围显示
        private void btnShowExtent_Click(object sender, EventArgs e)
        {
            moMap.FullExtent();
        }

        //选择
        private void btnSelect_Click(object sender, EventArgs e)
        {
            if (mOperatingLayerIndex == -1)
            {
                MessageBox.Show("请选中图层后,再进行选择要素操作!");
                return;
            }
            mMapOpStyle = 7;
            //this.Cursor = new Cursor("ico/EditSelect.ico");
            清除所选要素ToolStripMenuItem_Click(sender, e);
        }

        //放大
        private void btnZoomIn_Click(object sender, EventArgs e)
        {
            mMapOpStyle = 5;
        }

        //缩小
        private void btnZoomOut_Click(object sender, EventArgs e)
        {
            mMapOpStyle = 6;
        }

        //识别
        private void btnIdentify_Click(object sender, EventArgs e)
        {
            mMapOpStyle = 8;
        }

        //双击选择图层
        private void layersTree_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            mMapOpStyle = 0;
            foreach(TreeNode n in layersTree.Nodes)
            {
                n.BackColor = Color.Empty;
            }
            mLastOpLayerIndex = e.Node.Index;
            e.Node.BackColor = Color.LightGray;
        }

        #endregion

        #region 鼠标事件

        #region MouseDown
        private void moMap_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (mMapOpStyle == 1 && mOperatingLayerIndex != -1)
                {
                    OnEdit_MouseDown(e);
                }
                else if (mMapOpStyle == 2)
                {
                    ;
                }
                else if (mMapOpStyle == 3 && mOperatingLayerIndex != -1)
                {
                    OnEditPoint_MouseDown(e);
                }
                else if (mMapOpStyle == 4)
                {
                    OnPan_MouseDown(e);
                }
                else if (mMapOpStyle == 5)
                {
                    OnZoomIn_MouseDown(e);
                }
                else if (mMapOpStyle == 6)
                {
                    ;
                }
                else if (mMapOpStyle == 7)
                {
                    OnNoEditSelect_MouseDown(e);
                }
                else if (mMapOpStyle == 8)
                {
                    OnIdentify_MouseDown(e);
                }
            }
        }
        private void OnEdit_MouseDown(MouseEventArgs e)
        {
            //判断应该是进行选择还是移动
            mIsInMove = false;
            mIsInSelect = false;
            MyMapObjects.moPoint sPoint = moMap.ToMapPoint(e.Location.X, e.Location.Y);
            if (moMap.Layers.GetItem(mOperatingLayerIndex).ShapeType == MyMapObjects.moGeometryTypeConstant.MultiPolygon)
            {
                for (Int32 i = 0; i < moMap.Layers.GetItem(mOperatingLayerIndex).SelectedFeatures.Count; i++)
                {
                    MyMapObjects.moFeature sFeature = moMap.Layers.GetItem(mOperatingLayerIndex).SelectedFeatures.GetItem(i);
                    MyMapObjects.moMultiPolygon sMultiPolygon = (MyMapObjects.moMultiPolygon)sFeature.Geometry;
                    if (MyMapObjects.moMapTools.IsPointWithinMultiPolygon(sPoint, sMultiPolygon))
                    {
                        mIsInMove = true;
                        break;
                    }
                }
            }
            else if (moMap.Layers.GetItem(mOperatingLayerIndex).ShapeType == MyMapObjects.moGeometryTypeConstant.MultiPolyline)
            {
                for (Int32 i = 0; i < moMap.Layers.GetItem(mOperatingLayerIndex).SelectedFeatures.Count; i++)
                {
                    MyMapObjects.moFeature sFeature = moMap.Layers.GetItem(mOperatingLayerIndex).SelectedFeatures.GetItem(i);
                    MyMapObjects.moMultiPolyline sMultiPolyline = (MyMapObjects.moMultiPolyline)sFeature.Geometry;
                    double sTolerance = moMap.ToMapDistance(mSelectingTolerance);
                    if (MyMapObjects.moMapTools.IsPointOnMultiPolyline(sPoint, sMultiPolyline, sTolerance))
                    {
                        mIsInMove = true;
                        break;
                    }
                }
            }
            else if (moMap.Layers.GetItem(mOperatingLayerIndex).ShapeType == MyMapObjects.moGeometryTypeConstant.Point)
            {
                for (Int32 i = 0; i < moMap.Layers.GetItem(mOperatingLayerIndex).SelectedFeatures.Count; i++)
                {
                    MyMapObjects.moFeature sFeature = moMap.Layers.GetItem(mOperatingLayerIndex).SelectedFeatures.GetItem(i);
                    MyMapObjects.moPoint sFeaturePoint = (MyMapObjects.moPoint)sFeature.Geometry;
                    double sTolerance = moMap.ToMapDistance(mSelectingTolerance);
                    if (MyMapObjects.moMapTools.IsPointOnPoint(sPoint, sFeaturePoint, sTolerance))
                    {
                        mIsInMove = true;
                        break;
                    }
                }
            }
            else if (moMap.Layers.GetItem(mOperatingLayerIndex).ShapeType == MyMapObjects.moGeometryTypeConstant.MultiPoint)
            {
                for (Int32 i = 0; i < moMap.Layers.GetItem(mOperatingLayerIndex).SelectedFeatures.Count; i++)
                {
                    MyMapObjects.moFeature sFeature = moMap.Layers.GetItem(mOperatingLayerIndex).SelectedFeatures.GetItem(i);
                    MyMapObjects.moPoints sPoints = (MyMapObjects.moPoints)sFeature.Geometry;
                    double sTolerance = moMap.ToMapDistance(mSelectingTolerance);
                    if (MyMapObjects.moMapTools.IsPointOnPolyline(sPoint, sPoints, sTolerance))
                    {
                        mIsInMove = true;
                        break;
                    }
                }
            }
            if (mIsInMove)
            {
                OnMoveSelect_MouseDown(e);
            }
            else
            {
                mIsInSelect = true;
                OnSelect_MouseDown(e);
            }
        }
        private void OnSelect_MouseDown(MouseEventArgs e)
        {
            mStartMouseLocation = e.Location;
        }
        private void OnMoveSelect_MouseDown(MouseEventArgs e)
        {
            MyMapObjects.moMapLayer sLayer = moMap.Layers.GetItem(mOperatingLayerIndex);
            Int32 sSelFeatureCount = sLayer.SelectedFeatures.Count;
            if (sSelFeatureCount == 0) return;
            //复制图层
            mMovingGeometries.Clear();
            if (sLayer.ShapeType == MyMapObjects.moGeometryTypeConstant.MultiPolygon)
            {
                for (Int32 i = 0; i < sSelFeatureCount; ++i)
                {
                    MyMapObjects.moMultiPolygon sOriPolygon = (MyMapObjects.moMultiPolygon)sLayer.SelectedFeatures.GetItem(i).Geometry;
                    MyMapObjects.moMultiPolygon sDesPolygon = sOriPolygon.Clone();
                    mMovingGeometries.Add(sDesPolygon);
                }
            }
            else if (sLayer.ShapeType == MyMapObjects.moGeometryTypeConstant.MultiPolyline)
            {
                for (Int32 i = 0; i < sSelFeatureCount; ++i)
                {
                    MyMapObjects.moMultiPolyline sOriPolygon = (MyMapObjects.moMultiPolyline)sLayer.SelectedFeatures.GetItem(i).Geometry;
                    MyMapObjects.moMultiPolyline sDesPolygon = sOriPolygon.Clone();
                    mMovingGeometries.Add(sDesPolygon);
                }
            }
            else if (sLayer.ShapeType == MyMapObjects.moGeometryTypeConstant.Point)
            {
                for (Int32 i = 0; i < sSelFeatureCount; ++i)
                {
                    MyMapObjects.moPoint sOriPolygon = (MyMapObjects.moPoint)sLayer.SelectedFeatures.GetItem(i).Geometry;
                    MyMapObjects.moPoint sDesPolygon = sOriPolygon.Clone();
                    mMovingGeometries.Add(sDesPolygon);
                }
            }
            else if (sLayer.ShapeType == MyMapObjects.moGeometryTypeConstant.MultiPoint)
            {
                for (Int32 i = 0; i < sSelFeatureCount; ++i)
                {
                    MyMapObjects.moPoints sOriPolygon = (MyMapObjects.moPoints)sLayer.SelectedFeatures.GetItem(i).Geometry;
                    MyMapObjects.moPoints sDesPolygon = sOriPolygon.Clone();
                    mMovingGeometries.Add(sDesPolygon);
                }
            }
            //设置变量
            mStartMouseLocation = e.Location;
        }
        private void OnEditPoint_MouseDown(MouseEventArgs e)
        {
            if (mMouseOnPartIndex != -1 && mMouseOnPointIndex != -1)
            {
                mIsInEditPoint = true;
                if (mIsInMovePoint)
                {
                    mEditPointOperation.Add(1);
                    mEditPointRecord.Add(moMap.ToMapPoint(e.Location.X, e.Location.Y));
                    mPastPartIndex.Add(mMouseOnPartIndex);
                    mPastPointIndex.Add(mMouseOnPointIndex);
                }
            }
        }
        private void OnPan_MouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mStartMouseLocation = e.Location;
                mIsInPan = true;
                this.Cursor = new Cursor("ico/PanUp.ico");
            }
        }
        private void OnZoomIn_MouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mStartMouseLocation = e.Location;
                mIsInZoomIn = true;
                this.Cursor = new Cursor("ico/ZoomIn.ico");

            }
        }
        private void OnNoEditSelect_MouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mStartMouseLocation = e.Location;
                mIsInSelect = true;
            }
        }
        private void OnIdentify_MouseDown(MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                mStartMouseLocation = e.Location;
                mIsInIdentify = true;
            }
        }
        #endregion

        #region MouseMove
        private void moMap_MouseMove(object sender, MouseEventArgs e)
        {
            ShowCoordinate(e.Location);
            if (mMapOpStyle == 1)
            {
                OnEdit_MouseMove(e);
            }
            else if (mMapOpStyle == 2)
            {
                OnSketch_MouseMove(e);
            }
            else if (mMapOpStyle == 3)
            {
                OnEditPoint_MouseMove(e);
            }
            else if (mMapOpStyle == 4)
            {
                OnPan_MouseMove(e);
            }
            else if (mMapOpStyle == 5)
            {
                OnZoomIn_MouseMove(e);
            }
            else if (mMapOpStyle == 6)
            {
                ;
            }
            else if (mMapOpStyle == 7 && mOperatingLayerIndex != -1)
            {
                OnSelect_MouseMove(e);
            }
            else if (mMapOpStyle == 8)
            {
                OnIdentify_MouseMove(e);
            }
        }
        private void OnEdit_MouseMove(MouseEventArgs e)
        {
            if (mIsInMove)
            {
                OnMoveSelect_MouseMove(e);
            }
            else if (mIsInSelect)
            {
                OnSelect_MouseMove(e);
            }
        }
        private void OnSelect_MouseMove(MouseEventArgs e)
        {
            if (mIsInSelect == false)
            {
                return;
            }
            moMap.Refresh();
            MyMapObjects.moRectangle sRect = GetMapRectByTwoPoints(mStartMouseLocation, e.Location);
            MyMapObjects.moUserDrawingTool sDrawingTool = moMap.GetDrawingTool();
            sDrawingTool.DrawRectangle(sRect, mSelectBoxSymbol);
        }
        private void OnMoveSelect_MouseMove(MouseEventArgs e)
        {
            if (mIsInMove == false) return;
            //修改移动图形的坐标
            double sDeltaX = moMap.ToMapDistance(e.Location.X - mStartMouseLocation.X);
            double sDeltaY = moMap.ToMapDistance(mStartMouseLocation.Y - e.Location.Y);
            mNeedToSave = true;
            ModifyMovingGeometries(sDeltaX, sDeltaY);
            //刷新地图并绘制移动图形
            moMap.Refresh();
            DrawMovingShape();
            mSelectedIsMoved = true;
            //重新设置鼠标位置
            mStartMouseLocation = e.Location;
        }
        private void OnSketch_MouseMove(MouseEventArgs e)
        {
            MyMapObjects.moMapLayer sMapLayer = moMap.Layers.GetItem(mOperatingLayerIndex);
            MyMapObjects.moPoint sCurPoint = moMap.ToMapPoint(e.Location.X, e.Location.Y);
            if (sMapLayer.ShapeType == MyMapObjects.moGeometryTypeConstant.MultiPolygon)
            {
                MyMapObjects.moPoints sLastPart = mSketchingShape.Last();
                Int32 sPointCount = sLastPart.Count;
                if (sPointCount == 0)
                { }
                else if (sPointCount == 1)
                {
                    moMap.Refresh();
                    //只有一个顶点，则绘制一条橡皮筋
                    MyMapObjects.moPoint sFirstPoint = sLastPart.GetItem(0);
                    MyMapObjects.moUserDrawingTool sDrawingTool = moMap.GetDrawingTool();
                    sDrawingTool.DrawLine(sFirstPoint, sCurPoint, mElasticSymbol);
                }
                else
                {
                    moMap.Refresh();
                    //两个以上顶点，则绘制两条橡皮筋
                    MyMapObjects.moPoint sFirstPoint = sLastPart.GetItem(0);
                    MyMapObjects.moPoint sLastPoint = sLastPart.GetItem(sPointCount - 1);
                    MyMapObjects.moUserDrawingTool sDrawingTool = moMap.GetDrawingTool();
                    sDrawingTool.DrawLine(sFirstPoint, sCurPoint, mElasticSymbol);
                    sDrawingTool.DrawLine(sLastPoint, sCurPoint, mElasticSymbol);
                }
            }
            else if (sMapLayer.ShapeType == MyMapObjects.moGeometryTypeConstant.MultiPolyline)
            {
                MyMapObjects.moPoints sLastPart = mSketchingShape.Last();
                Int32 sPointCount = sLastPart.Count;
                if (sPointCount == 0)
                { }
                else
                {
                    moMap.Refresh();
                    MyMapObjects.moPoint sLastPoint = sLastPart.GetItem(sPointCount - 1);
                    MyMapObjects.moUserDrawingTool sDrawingTool = moMap.GetDrawingTool();
                    sDrawingTool.DrawLine(sLastPoint, sCurPoint, mElasticSymbol);
                }
            }
            else if (sMapLayer.ShapeType == MyMapObjects.moGeometryTypeConstant.Point)
            {
                ;
            }
            else if (sMapLayer.ShapeType == MyMapObjects.moGeometryTypeConstant.MultiPoint)
            {
                ;
            }
        }
        private void OnEditPoint_MouseMove(MouseEventArgs e)
        {
            MyMapObjects.moGeometryTypeConstant shapeType = moMap.Layers.GetItem(mOperatingLayerIndex).ShapeType;
            if (shapeType == MyMapObjects.moGeometryTypeConstant.MultiPolygon)
            {
                MultiPolygonEditing_MouseMove(e);
            }
            else if (shapeType == MyMapObjects.moGeometryTypeConstant.MultiPolyline)
            {
                MultiPolylineEditing_MouseMove(e);
            }
            else if (shapeType == MyMapObjects.moGeometryTypeConstant.Point)
            {
                PointEditing_MouseMove(e);
            }
            else if (shapeType == MyMapObjects.moGeometryTypeConstant.MultiPoint)
            {
                MultiPointEditing_MouseMove(e);
            }
        }
        private void MultiPolygonEditing_MouseMove(MouseEventArgs e)
        {
            if (mIsInEditPoint == false && mEditingGeometry != null)
            {
                MyMapObjects.moMultiPolygon sMultiPolygon = (MyMapObjects.moMultiPolygon)mEditingGeometry;
                double sTolerance = moMap.ToMapDistance(mSelectingTolerance);
                MyMapObjects.moPoint sPoint = moMap.ToMapPoint(e.Location.X, e.Location.Y);
                if (PointCloseToMultiPolygonPoint(sPoint, sMultiPolygon, sTolerance))
                {
                    if (mIsInMovePoint) this.Cursor = new Cursor("ico/EditMoveVertex.ico");
                    if (mIsInAddPoint) this.Cursor = new Cursor("ico/AddPoint.ico");
                    if (mIsInDeletePoint) this.Cursor = new Cursor("ico/DeletePoint.ico");
                }
                else
                {
                    this.Cursor = Cursors.Default;
                    mMouseOnPartIndex = -1;
                    mMouseOnPointIndex = -1;
                }
            }
            else
            {
                MyMapObjects.moMultiPolygon sMultiPolygon = (MyMapObjects.moMultiPolygon)mEditingGeometry;
                MyMapObjects.moPoint sPoint = moMap.ToMapPoint(e.Location.X, e.Location.Y);
                if (mIsInMovePoint)
                {
                    mPointEditNeedSave = true;
                    MyMapObjects.moPoint newPoint = sMultiPolygon.Parts.GetItem(mMouseOnPartIndex).GetItem(mMouseOnPointIndex);
                    newPoint.X = sPoint.X; newPoint.Y = sPoint.Y;
                    sMultiPolygon.UpdateExtent();
                    mEditingGeometry = sMultiPolygon;
                    moMap.RedrawTrackingShapes();
                }
                if (mIsInAddPoint) mIsInEditPoint = false;
                if (mIsInDeletePoint) mIsInEditPoint = false;
            }
        }
        private void MultiPolylineEditing_MouseMove(MouseEventArgs e)
        {
            if (mIsInEditPoint == false && mEditingGeometry != null)
            {
                MyMapObjects.moMultiPolyline sMultiPolyline = (MyMapObjects.moMultiPolyline)mEditingGeometry;
                double sTolerance = moMap.ToMapDistance(mSelectingTolerance);
                MyMapObjects.moPoint sPoint = moMap.ToMapPoint(e.Location.X, e.Location.Y);
                if (PointCloseToMultiPolylinePoint(sPoint, sMultiPolyline, sTolerance))
                {
                    if (mIsInMovePoint) this.Cursor = new Cursor("ico/EditMoveVertex.ico");
                    if (mIsInAddPoint) this.Cursor = new Cursor("ico/AddPoint.ico");
                    if (mIsInDeletePoint) this.Cursor = new Cursor("ico/DeletePoint.ico");
                }
                else
                {
                    this.Cursor = Cursors.Default;
                    mMouseOnPartIndex = -1;
                    mMouseOnPointIndex = -1;
                }
            }
            else
            {
                MyMapObjects.moMultiPolyline sMultiPolyline = (MyMapObjects.moMultiPolyline)mEditingGeometry;
                MyMapObjects.moPoint sPoint = moMap.ToMapPoint(e.Location.X, e.Location.Y);
                if (mIsInMovePoint)
                {
                    mPointEditNeedSave = true;
                    MyMapObjects.moPoint newPoint = sMultiPolyline.Parts.GetItem(mMouseOnPartIndex).GetItem(mMouseOnPointIndex);
                    newPoint.X = sPoint.X; newPoint.Y = sPoint.Y;
                    sMultiPolyline.UpdateExtent();
                    mEditingGeometry = sMultiPolyline;
                    moMap.RedrawTrackingShapes();
                }
                if (mIsInAddPoint) mIsInEditPoint = false;
                if (mIsInDeletePoint) mIsInEditPoint = false;
            }
        }
        private void PointEditing_MouseMove(MouseEventArgs e)
        {
            if (mIsInAddPoint || mIsInDeletePoint) return;
            if (mIsInEditPoint == false && mEditingGeometry != null)
            {
                MyMapObjects.moPoint sEditingPoint = (MyMapObjects.moPoint)mEditingGeometry;
                double sTolerance = moMap.ToMapDistance(mSelectingTolerance);
                MyMapObjects.moPoint sPoint = moMap.ToMapPoint(e.Location.X, e.Location.Y);
                if (MyMapObjects.moMapTools.IsPointOnPoint(sPoint, sEditingPoint, sTolerance))
                {
                    mMouseOnPartIndex = 0;
                    mMouseOnPointIndex = 0;
                    if (mIsInMovePoint) this.Cursor = new Cursor("ico/EditMoveVertex.ico");
                }
                else
                {
                    this.Cursor = Cursors.Default;
                    mMouseOnPartIndex = -1;
                    mMouseOnPointIndex = -1;
                }
            }
            else
            {
                MyMapObjects.moPoint sPoint = moMap.ToMapPoint(e.Location.X, e.Location.Y);
                if (mIsInMovePoint)
                {
                    mPointEditNeedSave = true;
                    mEditingGeometry = sPoint;
                    moMap.RedrawTrackingShapes();
                }
            }
        }
        private void MultiPointEditing_MouseMove(MouseEventArgs e)
        {
            if (mIsInEditPoint == false && mEditingGeometry != null)
            {
                MyMapObjects.moPoints sPoints = (MyMapObjects.moPoints)mEditingGeometry;
                double sTolerance = moMap.ToMapDistance(mSelectingTolerance);
                MyMapObjects.moPoint sPoint = moMap.ToMapPoint(e.Location.X, e.Location.Y);
                if (PointCloseToPoints(sPoint, sPoints, sTolerance))
                {
                    if (mIsInMovePoint) this.Cursor = new Cursor("ico/EditMoveVertex.ico");
                    if (mIsInAddPoint) this.Cursor = new Cursor("ico/AddPoint.ico");
                    if (mIsInDeletePoint) this.Cursor = new Cursor("ico/DeletePoint.ico");
                }
                else
                {
                    this.Cursor = Cursors.Default;
                    mMouseOnPartIndex = -1;
                    mMouseOnPointIndex = -1;
                }
            }
            else
            {
                MyMapObjects.moPoints sPoints = (MyMapObjects.moPoints)mEditingGeometry;
                MyMapObjects.moPoint sPoint = moMap.ToMapPoint(e.Location.X, e.Location.Y);
                if (mIsInMovePoint)
                {
                    mPointEditNeedSave = true;
                    MyMapObjects.moPoint newPoint = sPoints.GetItem(mMouseOnPointIndex);
                    newPoint.X = sPoint.X; newPoint.Y = sPoint.Y;
                    sPoints.UpdateExtent();
                    mEditingGeometry = sPoints;
                    moMap.RedrawTrackingShapes();
                }
                //if (mIsInAddPoint) mIsInEditPoint = false;
                if (mIsInDeletePoint) mIsInEditPoint = false;
            }
        }
        private void OnPan_MouseMove(MouseEventArgs e)
        {
            if (mIsInPan == false)
                return;
            moMap.PanMapImageTo(e.Location.X - mStartMouseLocation.X, e.Location.Y - mStartMouseLocation.Y);
        }
        private void OnZoomIn_MouseMove(MouseEventArgs e)
        {
            if (mIsInZoomIn == false)
            {
                return;
            }
            moMap.Refresh();
            MyMapObjects.moRectangle sRect = GetMapRectByTwoPoints(mStartMouseLocation, e.Location);
            MyMapObjects.moUserDrawingTool sDrawingTool = moMap.GetDrawingTool();
            sDrawingTool.DrawRectangle(sRect, mZoomBoxSymbol);
        }
        private void OnIdentify_MouseMove(MouseEventArgs e)
        {
            if (mIsInIdentify == false)
            {
                return;
            }
            moMap.Refresh();
            MyMapObjects.moRectangle sRect = GetMapRectByTwoPoints(mStartMouseLocation, e.Location);
            MyMapObjects.moUserDrawingTool sDrawingTool = moMap.GetDrawingTool();
            sDrawingTool.DrawRectangle(sRect, mSelectBoxSymbol);
        }
        #endregion

        #region MouseUp
        private void moMap_MouseUp(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (mMapOpStyle == 1 && mOperatingLayerIndex != -1)
                {
                    OnEdit_MouseUp(e);
                }
                else if (mMapOpStyle == 2)
                {
                    ;
                }
                else if (mMapOpStyle == 3 && mOperatingLayerIndex != -1)
                {
                    OnEditPoint_MouseUp(e);
                }
                else if (mMapOpStyle == 4)
                {
                    OnPan_MouseUp(e);
                }
                else if (mMapOpStyle == 5)
                {
                    OnZoomIn_MouseUp(e);
                }
                else if (mMapOpStyle == 6)
                {
                    ;
                }
                else if (mMapOpStyle == 7 && mOperatingLayerIndex != -1)
                {
                    OnSelect_MouseUp(e);
                }
                else if (mMapOpStyle == 8)
                {
                    OnIdentify_MouseUp(e);
                }
            }
        }
        private void OnEdit_MouseUp(MouseEventArgs e)
        {
            if (mIsInMove)
            {
                OnMoveSelect_MouseUp(e);
            }
            else if (mIsInSelect)
            {
                OnSelect_MouseUp(e);
            }
        }
        private void OnSelect_MouseUp(MouseEventArgs e)
        {
            if (mIsInSelect == false)
            {
                return;
            }
            mIsInSelect = false;
            MyMapObjects.moRectangle sBox = GetMapRectByTwoPoints(mStartMouseLocation, e.Location);
            double sTolerance = moMap.ToMapDistance(mSelectingTolerance);
            moMap.SelectLayerByBox(sBox, sTolerance, mOperatingLayerIndex); //该方法只在当前图层中选择，与demo中不同
            moMap.RedrawTrackingShapes();
            RedrawAttribute();
        }
        private void OnMoveSelect_MouseUp(MouseEventArgs e)
        {
            if (mIsInMove == false) return;
            mIsInMove = false;
            if (mSelectedIsMoved)
            {
                //做相应的数据修改
                mSelectedIsMoved = false;
                mNeedToSave = true;
                MyMapObjects.moMapLayer sLayer = moMap.Layers.GetItem(mOperatingLayerIndex);
                for (Int32 i = 0; i < sLayer.SelectedFeatures.Count; i++)
                {
                    MyMapObjects.moFeature sFeature = sLayer.SelectedFeatures.GetItem(i);
                    sFeature.Geometry = mMovingGeometries[i];
                }
                sLayer.UpdateExtent();
                //重构地图
                moMap.RedrawMap();
            }
            else
            {
                MyMapObjects.moRectangle sBox = GetMapRectByTwoPoints(e.Location, e.Location);
                double sTolerance = moMap.ToMapDistance(mSelectingTolerance);
                moMap.SelectLayerByBox(sBox, sTolerance, mOperatingLayerIndex); //该方法只在当前图层中选择，与demo中不同
                moMap.RedrawTrackingShapes();
            }
            //清除移动图形列表
            mMovingGeometries.Clear();
        }
        private void OnEditPoint_MouseUp(MouseEventArgs e)
        {
            if (mIsInEditPoint == false)
            {
                MoveFeatureBtn_Click(new object(), e);
            }
            if (mIsInAddPoint && moMap.Layers.GetItem(mOperatingLayerIndex).ShapeType != MyMapObjects.moGeometryTypeConstant.Point)
            {
                mEditPointOperation.Add(2);
                mPastPartIndex.Add(mMouseOnPartIndex);
                mPastPointIndex.Add(mMouseOnPointIndex);
            }
            MyMapObjects.moGeometryTypeConstant shapeType = moMap.Layers.GetItem(mOperatingLayerIndex).ShapeType;
            mIsInEditPoint = false;
            if (shapeType == MyMapObjects.moGeometryTypeConstant.MultiPolygon)
            {
                MultiPolygonEditing_MouseUp(e);
            }
            else if (shapeType == MyMapObjects.moGeometryTypeConstant.MultiPolyline)
            {
                MultiPolylineEditing_MouseUp(e);
            }
            else if (shapeType == MyMapObjects.moGeometryTypeConstant.Point)
            {
                PointEditing_MouseUp(e);
            }
            else if (shapeType == MyMapObjects.moGeometryTypeConstant.MultiPoint)
            {
                MultiPointEditing_MouseUp(e);
            }
            mMouseOnPartIndex = -1;
            mMouseOnPointIndex = -1;
        }
        private void MultiPolygonEditing_MouseUp(MouseEventArgs e)
        {
            MyMapObjects.moMultiPolygon sMultiPolygon = (MyMapObjects.moMultiPolygon)mEditingGeometry;
            MyMapObjects.moPoint sPoint = moMap.ToMapPoint(e.Location.X, e.Location.Y);
            if (mIsInAddPoint)
            {
                mPointEditNeedSave = true;
                MyMapObjects.moPoints sPoints = sMultiPolygon.Parts.GetItem(mMouseOnPartIndex);
                sPoints.Insert(mMouseOnPointIndex + 1, sPoint);
                sMultiPolygon.UpdateExtent();
                mEditingGeometry = sMultiPolygon;
                moMap.RedrawTrackingShapes();
                MovePointBtn_Click(new object(), e);
            }
            if (mIsInDeletePoint)
            {
                mPointEditNeedSave = true;
                MyMapObjects.moPoints sPoints = sMultiPolygon.Parts.GetItem(mMouseOnPartIndex);
                if (sPoints.Count > 3)
                {
                    mEditPointOperation.Add(3);
                    mEditPointRecord.Add(sPoints.GetItem(mMouseOnPointIndex).Clone());
                    mPastPartIndex.Add(mMouseOnPartIndex);
                    mPastPointIndex.Add(mMouseOnPointIndex);
                    sPoints.RemoveAt(mMouseOnPointIndex);
                }
                else
                {
                    MessageBox.Show("无法继续删减，否则会导致要素的几何无效！");
                    MovePointBtn_Click(new object(), e);
                    return;
                }
                sMultiPolygon.UpdateExtent();
                mEditingGeometry = sMultiPolygon;
                moMap.RedrawTrackingShapes();
            }
        }
        private void MultiPolylineEditing_MouseUp(MouseEventArgs e)
        {
            MyMapObjects.moMultiPolyline sMultiPolyline = (MyMapObjects.moMultiPolyline)mEditingGeometry;
            MyMapObjects.moPoint sPoint = moMap.ToMapPoint(e.Location.X, e.Location.Y);
            if (mIsInAddPoint)
            {
                mPointEditNeedSave = true;
                MyMapObjects.moPoints sPoints = sMultiPolyline.Parts.GetItem(mMouseOnPartIndex);
                sPoints.Insert(mMouseOnPointIndex + 1, sPoint);
                sMultiPolyline.UpdateExtent();
                mEditingGeometry = sMultiPolyline;
                moMap.RedrawTrackingShapes();
                MovePointBtn_Click(new object(), e);
            }
            if (mIsInDeletePoint)
            {
                mPointEditNeedSave = true;
                MyMapObjects.moPoints sPoints = sMultiPolyline.Parts.GetItem(mMouseOnPartIndex);
                if (sPoints.Count > 2)
                {
                    mEditPointOperation.Add(3);
                    mEditPointRecord.Add(sPoints.GetItem(mMouseOnPointIndex).Clone());
                    mPastPartIndex.Add(mMouseOnPartIndex);
                    mPastPointIndex.Add(mMouseOnPointIndex);
                    sPoints.RemoveAt(mMouseOnPointIndex);
                    sPoints.RemoveAt(mMouseOnPointIndex);
                }
                else
                {
                    MessageBox.Show("无法继续删减，否则会导致要素的几何无效！");
                    MovePointBtn_Click(new object(), e);
                    return;
                }
                sMultiPolyline.UpdateExtent();
                mEditingGeometry = sMultiPolyline;
                moMap.RedrawTrackingShapes();
            }
        }
        private void PointEditing_MouseUp(MouseEventArgs e)
        {
            if (mIsInAddPoint || mIsInDeletePoint)
            {
                MessageBox.Show("点要素无法执行当前操作！");
                MovePointBtn_Click(new object(), e);
            }
        }
        private void MultiPointEditing_MouseUp(MouseEventArgs e)
        {
            MyMapObjects.moPoints sPoints = (MyMapObjects.moPoints)mEditingGeometry;
            MyMapObjects.moPoint sPoint = moMap.ToMapPoint(e.Location.X, e.Location.Y);
            if (mIsInAddPoint)
            {
                mPointEditNeedSave = true;
                sPoints.Insert(mMouseOnPointIndex, sPoint);
                sPoints.UpdateExtent();
                mEditingGeometry = sPoints;
                moMap.RedrawTrackingShapes();
            }
            if (mIsInDeletePoint)
            {
                mPointEditNeedSave = true;
                if (sPoints.Count > 1)
                {
                    mEditPointOperation.Add(3);
                    mEditPointRecord.Add(sPoints.GetItem(mMouseOnPointIndex).Clone());
                    mPastPartIndex.Add(mMouseOnPartIndex);
                    mPastPointIndex.Add(mMouseOnPointIndex);
                    sPoints.RemoveAt(mMouseOnPointIndex);
                    sPoints.RemoveAt(mMouseOnPointIndex);
                }
                else
                {
                    MessageBox.Show("无法继续删减，否则会导致要素的几何无效！");
                    MovePointBtn_Click(new object(), e);
                    return;
                }
                sPoints.UpdateExtent();
                mEditingGeometry = sPoints;
                moMap.RedrawTrackingShapes();
            }
        }
        private void OnPan_MouseUp(MouseEventArgs e)
        {
            if (mIsInPan == false)
                return;
            mIsInPan = false;
            double sDeltaX = moMap.ToMapDistance(e.Location.X - mStartMouseLocation.X);
            double sDeltaY = moMap.ToMapDistance(mStartMouseLocation.Y - e.Location.Y);
            moMap.PanDelta(sDeltaX, sDeltaY);
            this.Cursor = System.Windows.Forms.Cursors.Default;
        }
        private void OnZoomIn_MouseUp(MouseEventArgs e)
        {
            if (mIsInZoomIn == false)
            {
                return;
            }
            mIsInZoomIn = false;
            if (mStartMouseLocation.X == e.Location.X && mStartMouseLocation.Y == e.Location.Y)
            {
                MyMapObjects.moPoint sPoint = moMap.ToMapPoint(mStartMouseLocation.X, mStartMouseLocation.Y);
                moMap.ZoomByCenter(sPoint, mZoomRatioFixed);
            }
            else
            {
                MyMapObjects.moRectangle sBox = GetMapRectByTwoPoints(mStartMouseLocation, e.Location);
                moMap.ZoomToExtent(sBox);
            }
            this.Cursor = System.Windows.Forms.Cursors.Default;

        }
        private void OnIdentify_MouseUp(MouseEventArgs e)
        {
            if (mIsInIdentify == false)
            {
                return;
            }
            mIsInIdentify = false;
            moMap.Refresh();
            MyMapObjects.moRectangle sBox = GetMapRectByTwoPoints(mStartMouseLocation, e.Location);
            double sTolerance = moMap.ToMapDistance(mSelectingTolerance);
            if (moMap.Layers.Count == 0)
            {
                return;
            }
            else
            {
                MyMapObjects.moMapLayer sLayer = moMap.Layers.GetItem(mOperatingLayerIndex);
                MyMapObjects.moFeatures sFeatures = sLayer.SearchByBox(sBox, sTolerance);
                Int32 sSelFeatureCount = sFeatures.Count;
                if (sSelFeatureCount > 0)
                {
                    MyMapObjects.moGeometry[] sGeometries = new MyMapObjects.moGeometry[sSelFeatureCount];
                    for (Int32 i = 0; i < sSelFeatureCount; i++)
                    {
                        sGeometries[i] = sFeatures.GetItem(i).Geometry;
                    }
                    moMap.FlashShapes(sGeometries, 5, 800);

                }
                if (sSelFeatureCount > 0)
                {
                    Identify mIdentify = new Identify(moMap.Layers.GetItem(mLastOpLayerIndex), sFeatures);
                    mIdentify.Owner = this;
                    mIdentify.ShowDialog();
                }
            }
        }
        #endregion

        #region MouseClick,MouseDoubleClick,MouseWheel
        private void moMap_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                if (mMapOpStyle == 1)
                {
                    ;
                }
                else if (mMapOpStyle == 2 && mOperatingLayerIndex != -1)
                {
                    OnSketch_MouseClick(e);
                }
                else if (mMapOpStyle == 4)
                {
                    ;
                }
                else if (mMapOpStyle == 5)
                {
                    ;
                }
                else if (mMapOpStyle == 6)
                {
                    OnZoomOut_MouseClick(e);
                }
                else if (mMapOpStyle == 7)
                {
                    ;
                }
            }
        }
        private void OnSketch_MouseClick(MouseEventArgs e)
        {
            MyMapObjects.moMapLayer sMapLayer = moMap.Layers.GetItem(mOperatingLayerIndex);
            if (sMapLayer.ShapeType == MyMapObjects.moGeometryTypeConstant.Point)
            {
                //将屏幕坐标转换为地图坐标并加入描绘图形
                MyMapObjects.moPoint sPoint = moMap.ToMapPoint(e.Location.X, e.Location.Y);
                mSketchingPoint.Add(sPoint);
                EndSketchGeo(sMapLayer.ShapeType);
                //地图控件重绘跟踪图层
                moMap.RedrawTrackingShapes();
            }
            else if (sMapLayer.ShapeType == MyMapObjects.moGeometryTypeConstant.MultiPoint)
            {
                //将屏幕坐标转换为地图坐标并加入描绘图形
                MyMapObjects.moPoint sPoint = moMap.ToMapPoint(e.Location.X, e.Location.Y);
                mSketchingPoint.Add(sPoint);
                EndSketchPart(sMapLayer.ShapeType);
                //地图控件重绘跟踪图层
                moMap.RedrawTrackingShapes();
            }
            else
            {
                //将屏幕坐标转换为地图坐标并加入描绘图形
                MyMapObjects.moPoint sPoint = moMap.ToMapPoint(e.Location.X, e.Location.Y);
                mSketchingShape.Last().Add(sPoint);
                //地图控件重绘跟踪图层
                moMap.RedrawTrackingShapes();
            }
        }
        private void OnZoomOut_MouseClick(MouseEventArgs e)
        {
            this.Cursor = new Cursor("ico/ZoomOut.ico");

            //单点缩小
            MyMapObjects.moPoint sPoint = moMap.ToMapPoint(e.Location.X, e.Location.Y);
            moMap.ZoomByCenter(sPoint, 1 / mZoomRatioFixed);
            this.Cursor = System.Windows.Forms.Cursors.Default;
        }
        private void moMap_MouseDoubleClick(object sender, MouseEventArgs e)
        {
            if (mOperatingLayerIndex != -1)
            {
                if (mMapOpStyle == 2)
                {
                    OnSketch_MouseDoubleClick(e);
                }
            }
        }
        private void OnSketch_MouseDoubleClick(MouseEventArgs e)
        {
            MyMapObjects.moMapLayer sMapLayer = moMap.Layers.GetItem(mOperatingLayerIndex);
            EndSketchPart(sMapLayer.ShapeType);
            EndSketchGeo(sMapLayer.ShapeType);
        }
        private void moMap_MouseWheel(object sender, MouseEventArgs e)
        {
            double sX = moMap.ClientRectangle.Width / 2;
            double sY = moMap.ClientRectangle.Height / 2;
            MyMapObjects.moPoint sPoint = moMap.ToMapPoint(sX, sY);
            if (e.Delta > 0)
                moMap.ZoomByCenter(sPoint, mZoomRatioMouseWheel);
            else
                moMap.ZoomByCenter(sPoint, 1 / mZoomRatioMouseWheel);
        }
        #endregion

        #endregion

        #region 右键菜单、图层树
        private void moMap_AfterTrackingLayerDraw(object sender, MyMapObjects.moUserDrawingTool drawingTool)
        {
            DrawSketchingShapes(drawingTool);   //绘制描绘图形
            DrawEditingShapes(drawingTool); //绘制正在编辑的图形
        }

        private void moMap_MapScaleChanged(object sender)
        {
            ShowMapScale();
        }

        private void moMapRightMenu_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            if (mMapOpStyle == 1)
            {
                RightOperateInSelect(e);
            }
            else if (mMapOpStyle == 2)
            {
                RightOperateInSketch(e);
            }
            else if (mMapOpStyle == 3)
            {
                RightOperateInEditPoint(e);
            }
        }

        private void moMapRightMenu_VisibleChanged(object sender, EventArgs e)
        {
            if (mOperatingLayerIndex == -1) return;
            if (mMapOpStyle == 1)
            {
                MyMapObjects.moMapLayer sLayer = moMap.Layers.GetItem(mOperatingLayerIndex);
                if (sLayer.SelectedFeatures.Count == 0)
                {
                    moMapRightMenu.Items[0].Enabled = false;
                    moMapRightMenu.Items[2].Enabled = false;
                    moMapRightMenu.Items[3].Enabled = false;
                }
                else
                {
                    moMapRightMenu.Items[0].Enabled = true;
                    moMapRightMenu.Items[2].Enabled = true;
                    moMapRightMenu.Items[3].Enabled = true;
                }
                if (mCopyingGeometries.Count == 0) moMapRightMenu.Items[1].Enabled = false;
                else moMapRightMenu.Items[1].Enabled = true;
            }
            if (mMapOpStyle == 2)
            {
                if (mSketchingPoint.Count == 0 && mSketchingShape.Count == 1 && mSketchingShape[0].Count == 0)
                {
                    for (Int32 i = 0; i < 5; i++) moMapRightMenu.Items[i].Enabled = false;
                }
                else
                {
                    for (Int32 i = 0; i < 5; i++) moMapRightMenu.Items[i].Enabled = true;
                }
            }
            if (mMapOpStyle == 3)
            {
                if (mEditPointOperation.Count == 0)
                {
                    moMapRightMenu.Items[1].Enabled = false;
                }
                else moMapRightMenu.Items[1].Enabled = true;
            }
        }

        private void 新建图层ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            CreateLayer newLayer = new CreateLayer();
            newLayer.Owner = this;
            newLayer.ShowDialog();
        }

        //在该函数中接收新建图层的参数
        public void GetCreateLayerInfo(string layerName, MyMapObjects.moGeometryTypeConstant layerType, string savePath)
        {
            string sGvFilePath = savePath; //用户输入的存储路径(包含文件名，以.gvshp结尾);
            if (sGvFilePath.IndexOf(".gvshp") == -1) sGvFilePath += ".gvshp";
            MyMapObjects.moGeometryTypeConstant sGeometryType = layerType;  //用户指定的图层要素类型
            string sDbfFilePath = sGvFilePath.Substring(0, sGvFilePath.IndexOf(".gvshp")) + ".gvdbf";
            //初始化文件读取类
            DataIOTools.gvShpFileManager sGvShpFileManager = new DataIOTools.gvShpFileManager(sGeometryType);
            sGvShpFileManager.SourceFileType = "gvshp";
            sGvShpFileManager.DefaultFilePath = sGvFilePath;
            sGvShpFileManager.SaveToFile(sGvShpFileManager.DefaultFilePath);
            DataIOTools.dbfFileManager sDbfFileManager = new DataIOTools.dbfFileManager();
            sDbfFileManager.DefaultPath = sDbfFilePath;
            MyMapObjects.moField sField = new MyMapObjects.moField("id", MyMapObjects.moValueTypeConstant.dInt32);  //为用户添加id字段
            sDbfFileManager.CreateField(sField, new MyMapObjects.moAttributes());
            sDbfFileManager.SaveToFile(sDbfFileManager.DefaultPath);
            //添加至图层并加载
            MyMapObjects.moFields sFields = new MyMapObjects.moFields();
            sFields.Append(sField);
            sFields.PrimaryField = sField.Name;
            MyMapObjects.moMapLayer sMapLayer = new MyMapObjects.moMapLayer(layerName, sGeometryType, sFields);
            //加载要素
            MyMapObjects.moFeatures sFeatures = new MyMapObjects.moFeatures();
            sMapLayer.Features = sFeatures;

            ThingsAfterNewLayer(sMapLayer, sGvShpFileManager, sDbfFileManager);
        }

        private void 打开地图ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            OpenFileDialog sDialog = new OpenFileDialog();
            sDialog.Filter = "图层文件|*.shp;*.gvshp";
            string sFileName = "";
            if (sDialog.ShowDialog(this) == DialogResult.OK)
            {
                sFileName = sDialog.FileName;
                sDialog.Dispose();
            }
            else
            {
                sDialog.Dispose();
                return;
            }
            try
            {
                //检查是否存在同名图层
                string layerName = Path.GetFileNameWithoutExtension(sFileName);
                for (Int32 i = 0; i < moMap.Layers.Count; i++)
                {
                    if (layerName == moMap.Layers.GetItem(i).Name)
                    {
                        string errorMsg = "已存在同名图层！";
                        throw new Exception(errorMsg);
                    }
                }
                string extension = Path.GetExtension(sFileName);
                string dbfFilePath = "";
                DataIOTools.gvShpFileManager sGvShpFileManager;
                List<MyMapObjects.moGeometry> sGeometries;
                MyMapObjects.moGeometryTypeConstant sGeometryType;
                if (extension == ".shp")
                {
                    //读取shp文件
                    string shpFilePath = sFileName;
                    dbfFilePath = shpFilePath.Substring(0, shpFilePath.IndexOf(".shp")) + ".dbf";
                    //(1)读取shp文件，并以gvshp文件进行管理
                    DataIOTools.shpFileReader sShpFileReader = new DataIOTools.shpFileReader(shpFilePath);
                    sGeometryType = sShpFileReader.ShapeType;
                    sGeometries = sShpFileReader.Geometries;
                    sGvShpFileManager = new DataIOTools.gvShpFileManager(sGeometryType);
                    sGvShpFileManager.SourceFileType = "shp";   //设置文件源
                    sGvShpFileManager.DefaultFilePath = shpFilePath;
                    sGvShpFileManager.UpdateGeometries(sGeometries);
                }
                else
                {
                    //读取gvshp文件
                    string gvshpFilePath = sFileName;
                    dbfFilePath = gvshpFilePath.Substring(0, gvshpFilePath.IndexOf(".gvshp")) + ".gvdbf";
                    //(1)读取gvshp文件
                    sGvShpFileManager = new DataIOTools.gvShpFileManager(gvshpFilePath);
                    sGvShpFileManager.SourceFileType = "gvshp";
                    sGvShpFileManager.DefaultFilePath = gvshpFilePath;
                    sGeometries = sGvShpFileManager.Geometries;
                    sGeometryType = sGvShpFileManager.GeometryType;
                }

                //(2)读取dbf文件
                DataIOTools.dbfFileManager sDbfFileManager = new DataIOTools.dbfFileManager(dbfFilePath);
                MyMapObjects.moFields sFields = sDbfFileManager.Fields;
                List<MyMapObjects.moAttributes> sAttributes = sDbfFileManager.AttributesList;
                //(3)判断要素数与属性数是否一致
                if (sGeometries.Count != sAttributes.Count)
                {
                    string errorMsg = "要素数与属性数不一致!";
                    throw new Exception(errorMsg);
                }
                //设置主字段
                for (Int32 i = 0; i < sFields.Count; i++)
                {
                    if (sFields.GetItem(i).Name == "id" && sFields.GetItem(i).ValueType == MyMapObjects.moValueTypeConstant.dInt32)
                    {
                        sFields.PrimaryField = "id";
                    }
                }
                //(4)添加至图层并加载
                MyMapObjects.moMapLayer sMapLayer = new MyMapObjects.moMapLayer(layerName, sGeometryType, sFields);
                //加载要素
                MyMapObjects.moFeatures sFeatures = new MyMapObjects.moFeatures();
                for (Int32 i = 0; i < sGeometries.Count; ++i)
                {
                    MyMapObjects.moFeature sFeature = new MyMapObjects.moFeature(sGeometryType, sGeometries[i], sAttributes[i]);
                    sFeatures.Add(sFeature);
                }
                sMapLayer.Features = sFeatures;
                index_dbf += 1;
                index_gvshp += 1;
                ThingsAfterNewLayer(sMapLayer, sGvShpFileManager, sDbfFileManager);
            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString());
                return;
            }
        }

        private void layersTree_MouseDown(object sender, MouseEventArgs e)
        {
            Point clickPoint = new Point(e.X, e.Y);
            TreeNode currentNode = layersTree.GetNodeAt(clickPoint);
            if (currentNode != null)
            {
                layersTree.SelectedNode = currentNode;
                mLastOpLayerIndex = currentNode.Index;  //鼠标单击或右键菜单对应的图层索引
            }
            else
            {
                if (e.Button == MouseButtons.Right)
                {
                    mLastOpLayerIndex = -1;
                }
            }
        }

        private void layersTree_DragDrop(object sender, DragEventArgs e)
        {
            if (layersTree.Nodes.Count <= 1) //结点数量少于2
            {
                return;
            }
            TreeNode moveNode = (TreeNode)e.Data.GetData("System.Windows.Forms.TreeNode");
            //根据鼠标坐标确定要移动到的目标节点
            int sIndex1 = moveNode.Index;
            Point pt = ((TreeView)(sender)).PointToClient(new Point(e.X, e.Y));
            //寻找目标结点号
            int sIndex2 = 0;
            int sNodesNum = layersTree.Nodes.Count;
            Rectangle sFirstRect = layersTree.Nodes[0].Bounds;
            Rectangle sLastRect = layersTree.Nodes[sNodesNum - 1].Bounds;
            if (pt.Y <= sFirstRect.Y)
            {
                sIndex2 = 0;
            }
            else if (pt.Y >= (sLastRect.Y + sLastRect.Height))
            {
                sIndex2 = sNodesNum - 1;
            }
            else
            {
                for (int i = 0; i < sNodesNum; ++i)
                {
                    Rectangle sCurRect = layersTree.Nodes[i].Bounds;
                    if ((pt.Y > sCurRect.Y) && (pt.Y <= (sCurRect.Y + sCurRect.Height)))
                    {
                        sIndex2 = i;
                        break;
                    }
                }
            }
            if (sIndex1 == sIndex2) //没有交换顺序
            {
                return;
            }
            MyMapObjects.moMapLayer sMaplayer1 = moMap.Layers.GetItem(sIndex1);
            DataIOTools.gvShpFileManager sGvShp1 = mGvShapeFiles[sIndex1];
            DataIOTools.dbfFileManager sDbf1 = mDbfFiles[sIndex1];
            MyMapObjects.moMapLayer sMaplayer2 = moMap.Layers.GetItem(sIndex2);
            DataIOTools.gvShpFileManager sGvShp2 = mGvShapeFiles[sIndex2];
            DataIOTools.dbfFileManager sDbf2 = mDbfFiles[sIndex2];
            moMap.Layers.RemoveAt(sIndex1);
            mGvShapeFiles.RemoveAt(sIndex1);
            mDbfFiles.RemoveAt(sIndex1);
            moMap.Layers.Insert(sIndex1, sMaplayer2);
            mGvShapeFiles.Insert(sIndex1, sGvShp2);
            mDbfFiles.Insert(sIndex1, sDbf2);
            moMap.Layers.RemoveAt(sIndex2);
            mGvShapeFiles.RemoveAt(sIndex2);
            mDbfFiles.RemoveAt(sIndex2);
            moMap.Layers.Insert(sIndex2, sMaplayer1);
            mGvShapeFiles.Insert(sIndex2, sGvShp1);
            mDbfFiles.Insert(sIndex2, sDbf1);
            RefreshLayersTree();    //刷新图层列表
            moMap.RedrawMap();  //刷新地图
        }

        private void layersTree_DragEnter(object sender, DragEventArgs e)
        {
            if (e.Data.GetDataPresent("System.Windows.Forms.TreeNode"))
            {
                e.Effect = DragDropEffects.Move;
            }
            else
            {
                e.Effect = DragDropEffects.None;
            }
        }

        private void layersTree_ItemDrag(object sender, ItemDragEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                DoDragDrop(e.Item, DragDropEffects.Move);
            }
        }

        private void layersTree_AfterCheck(object sender, TreeViewEventArgs e)
        {
            Int32 sIndex = e.Node.Index;
            MyMapObjects.moMapLayer sMapLayer = moMap.Layers.GetItem(sIndex);
            sMapLayer.Visible = e.Node.Checked;
            bool forceToEnd = false;
            if (e.Node.Index == mOperatingLayerIndex && SelectLayer.Enabled == true)
            {
                EndEditItem_Click(sender, e);
                forceToEnd = true;
            }
            if (SelectLayer.Enabled == true) RefreshSelectLayer();
            moMap.RedrawMap();
            if (forceToEnd) BeginEditItem_Click(sender, e);
        }

        private void 移除ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mEditMoMap == true && mOperatingLayerIndex == mLastOpLayerIndex)
            {
                EndEditItem_Click(sender, e);
                moMap.Layers.RemoveAt(mLastOpLayerIndex);
                if (index_gvshp != 0)
                    mGvShapeFiles.RemoveAt(index_gvshp);
                if (index_dbf != 0)
                    mDbfFiles.RemoveAt(index_dbf);
                mLastOpLayerIndex = -1;
                RefreshLayersTree();
                RefreshSelectLayer();
                moMap.RedrawMap();
            }
            else
            {
                moMap.Layers.RemoveAt(mLastOpLayerIndex);
                if (index_gvshp != 0)
                    mGvShapeFiles.RemoveAt(index_gvshp);
                if (index_dbf != 0)
                    mDbfFiles.RemoveAt(index_dbf);
                mLastOpLayerIndex = -1;
                RefreshLayersTree();
                moMap.RedrawMap();
            }
        }

        private void 打开属性表ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            AttributeTable datafram_windows = new AttributeTable(this, mLastOpLayerIndex);
            datafram_windows.Owner = this;
            datafram_windows.Name = moMap.Layers.GetItem(mLastOpLayerIndex).Name;
            datafram_windows.Show();
            datafram_windows.SetDesktopLocation(this.Location.X + (this.Width - datafram_windows.Width) / 2,
                this.Location.Y + (this.Height - datafram_windows.Height) / 2);
            datafram_windows.Refresh_dataform_select();
            All_tables.Add(datafram_windows);//将新打开的添加进去
            datafram_windows.Windows_index = Fid_table_windows;
            Fid_table_windows++;
        }
        private void 按属性选择ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Research research_select = new Research(this);//导入本窗口就可以了
            research_select.Owner = this;

            research_select.Show();//展示
        }

        private void 缩放至图层ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MyMapObjects.moMapLayer sLayer = moMap.Layers.GetItem(mLastOpLayerIndex);
            sLayer.UpdateExtent();
            MyMapObjects.moRectangle sRect = sLayer.Extent;
            moMap.SetExtent(sRect);
        }

        private void 编辑要素ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (SelectLayer.Enabled == true)    //编辑状态
            {
                if (mOperatingLayerIndex != mLastOpLayerIndex) //更换编辑图层
                {
                    EndEditItem_Click(sender, e);
                }
            }
            BeginEditItem_Click(sender, e);
        }

        private void 属性ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MyMapObjects.moMapLayer sLayer = moMap.Layers.GetItem(mLastOpLayerIndex);
            LayerAttributes newLayer = new LayerAttributes();
            newLayer.Owner = this;

            string text = "";
            text += "图层名称：" + sLayer.Name + "\n";
            string shapeType = "Point";
            if (sLayer.ShapeType == MyMapObjects.moGeometryTypeConstant.MultiPolygon) shapeType = "MultiPolygon";
            else if (sLayer.ShapeType == MyMapObjects.moGeometryTypeConstant.MultiPolyline) shapeType = "MultiPolyline";
            else if (sLayer.ShapeType == MyMapObjects.moGeometryTypeConstant.MultiPoint) shapeType = "MultiPoint";
            text += "要素类型：" + shapeType + "\n";
            text += "要素数量：" + sLayer.Features.Count + "\n";
            text += "字段数量：" + sLayer.AttributeFields.Count;

            newLayer.SetText(text);
            newLayer.ShowDialog();
        }

        private void 图层渲染ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            mIsInRenderer = false;
            MyMapObjects.moMapLayer sLayer = moMap.Layers.GetItem(mLastOpLayerIndex);//待渲染的图层
            if (sLayer.ShapeType == MyMapObjects.moGeometryTypeConstant.Point || sLayer.ShapeType == MyMapObjects.moGeometryTypeConstant.MultiPoint)
            {
                mPointRenderer = new PointRenderer(moMap.Layers.GetItem(mLastOpLayerIndex));
                mPointRenderer.Owner = this;
                mPointRenderer.ShowDialog();
                if (mIsInRenderer == false)
                {
                    return;
                }
                //简单渲染
                if (mPointRendererMode == 0)
                {
                    MyMapObjects.moSimpleRenderer sRenderer = new MyMapObjects.moSimpleRenderer();
                    MyMapObjects.moSimpleMarkerSymbol sSymbol = new MyMapObjects.moSimpleMarkerSymbol();
                    sSymbol.Style = (MyMapObjects.moSimpleMarkerSymbolStyleConstant)mPointSymbolStyle;//修改样式
                    sSymbol.Color = mPointSimpleRendererColor;//修改颜色
                    sSymbol.Size = mPointSimpleRendererSize;//修改尺寸
                    sRenderer.Symbol = sSymbol;
                    sLayer.Renderer = sRenderer;
                    moMap.RedrawMap();
                }
                //唯一值渲染
                else if (mPointRendererMode == 1)
                {
                    MyMapObjects.moUniqueValueRenderer sRenderer = new MyMapObjects.moUniqueValueRenderer();
                    sRenderer.Field = sLayer.AttributeFields.GetItem(mPointUniqueFieldIndex).Name;
                    List<string> sValues = new List<string>();
                    Int32 sFeatrueCount = sLayer.Features.Count;
                    for (Int32 i = 0; i < sFeatrueCount; i++) //加入所有要素的属性值
                    {
                        string sValue = Convert.ToString(sLayer.Features.GetItem(i).Attributes.GetItem(mPointUniqueFieldIndex));
                        sValues.Add(sValue);
                    }
                    //去除重复
                    sValues = sValues.Distinct().ToList();
                    //生成符号
                    Int32 sValueCount = sValues.Count;
                    for (Int32 i = 0; i < sValueCount; i++)
                    {
                        MyMapObjects.moSimpleMarkerSymbol sSymbol = new MyMapObjects.moSimpleMarkerSymbol();
                        sSymbol.Style = (MyMapObjects.moSimpleMarkerSymbolStyleConstant)mPointSymbolStyle;//修改样式
                        sSymbol.Size = mPointSimpleRendererSize;//修改尺寸
                        sRenderer.AddUniqueValue(sValues[i], sSymbol);
                    }
                    sRenderer.DefaultSymbol = new MyMapObjects.moSimpleMarkerSymbol();
                    sLayer.Renderer = sRenderer;
                    moMap.RedrawMap();
                }
                //分级渲染
                else if (mPointRendererMode == 2)
                {
                    MyMapObjects.moClassBreaksRenderer sRenderer = new MyMapObjects.moClassBreaksRenderer();
                    sRenderer.Field = sLayer.AttributeFields.GetItem(mPointClassBreaksFieldIndex).Name;
                    List<double> sValues = new List<double>();
                    Int32 sFeatrueCount = sLayer.Features.Count;
                    Int32 sFieldIndex = sLayer.AttributeFields.FindField(sRenderer.Field);
                    MyMapObjects.moValueTypeConstant sValueType = sLayer.AttributeFields.GetItem(sFieldIndex).ValueType;
                    if (sValueType == MyMapObjects.moValueTypeConstant.dText)
                    {
                        MessageBox.Show("该字段不是数值字段，不支持分级渲染！");
                        return;
                    }
                    try
                    {
                        for (Int32 i = 0; i < sFeatrueCount; i++)
                        {
                            double sValue = Convert.ToDouble(sLayer.Features.GetItem(i).Attributes.GetItem(sFieldIndex));
                            sValues.Add(sValue);
                        }
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("该字段不是数值字段，不支持分级渲染！");
                        return;
                    }
                    double sMinValue = sValues.Min();
                    double sMaxValue = sValues.Max();
                    for (Int32 i = 0; i < mPointClassBreaksNum; i++)
                    {
                        double sValue = sMinValue + (sMaxValue - sMinValue) * (i + 1) / mPointClassBreaksNum;
                        MyMapObjects.moSimpleMarkerSymbol sSymbol = new MyMapObjects.moSimpleMarkerSymbol();
                        sSymbol.Color = mPointClassBreaksRendererColor;
                        sSymbol.Style = (MyMapObjects.moSimpleMarkerSymbolStyleConstant)mPointSymbolStyle;
                        sRenderer.AddBreakValue(sValue, sSymbol);
                    }
                    double sMinSize = mPointClassBreaksRendererMinSize;
                    double sMaxSize = mPointClassBreaksRendererMaxSize;
                    sRenderer.RampSize(sMinSize, sMaxSize);
                    sRenderer.DefaultSymbol = new MyMapObjects.moSimpleMarkerSymbol();
                    sLayer.Renderer = sRenderer;
                    moMap.RedrawMap();
                }
            }
            else if (sLayer.ShapeType == MyMapObjects.moGeometryTypeConstant.MultiPolyline)
            {
                mPolylineRenderer = new PolylineRenderer(moMap.Layers.GetItem(mLastOpLayerIndex));
                mPolylineRenderer.Owner = this;
                mPolylineRenderer.ShowDialog();
                if (mIsInRenderer == false)
                {
                    return;
                }
                //简单渲染
                if (mPolylineRendererMode == 0)
                {
                    MyMapObjects.moSimpleRenderer sRenderer = new MyMapObjects.moSimpleRenderer();
                    MyMapObjects.moSimpleLineSymbol sSymbol = new MyMapObjects.moSimpleLineSymbol();
                    sSymbol.Style = (MyMapObjects.moSimpleLineSymbolStyleConstant)mPolylineSymbolStyle;//传参修改
                    sSymbol.Color = mPolylineSimpleRendererColor;//修改颜色
                    sSymbol.Size = mPolylineSimpleRendererSize;//修改尺寸
                    sRenderer.Symbol = sSymbol;
                    sLayer.Renderer = sRenderer;
                    moMap.RedrawMap();
                }
                //唯一值渲染
                else if (mPolylineRendererMode == 1)
                {
                    MyMapObjects.moUniqueValueRenderer sRenderer = new MyMapObjects.moUniqueValueRenderer();
                    sRenderer.Field = sLayer.AttributeFields.GetItem(mPolylineUniqueFieldIndex).Name;
                    List<string> sValues = new List<string>();
                    Int32 sFeatrueCount = sLayer.Features.Count;
                    for (Int32 i = 0; i < sFeatrueCount; i++)
                    {
                        string sValue = Convert.ToString(sLayer.Features.GetItem(i).Attributes.GetItem(mPolylineUniqueFieldIndex));
                        sValues.Add(sValue);
                    }
                    //去除重复
                    sValues = sValues.Distinct().ToList();
                    //生成符号
                    Int32 sValueCount = sValues.Count;
                    for (Int32 i = 0; i < sValueCount; i++)
                    {
                        MyMapObjects.moSimpleLineSymbol sSymbol = new MyMapObjects.moSimpleLineSymbol();
                        sSymbol.Style = (MyMapObjects.moSimpleLineSymbolStyleConstant)mPolylineSymbolStyle;//修改样式
                        sSymbol.Size = mPolylineUniqueRendererSize;//修改尺寸
                        sRenderer.AddUniqueValue(sValues[i], sSymbol);
                    }
                    sRenderer.DefaultSymbol = new MyMapObjects.moSimpleLineSymbol();
                    sLayer.Renderer = sRenderer;
                    moMap.RedrawMap();
                }
                //分级渲染
                else if (mPolylineRendererMode == 2)
                {
                    MyMapObjects.moClassBreaksRenderer sRenderer = new MyMapObjects.moClassBreaksRenderer();
                    sRenderer.Field = sLayer.AttributeFields.GetItem(mPolylineClassBreaksFieldIndex).Name;
                    List<double> sValues = new List<double>();
                    Int32 sFeatrueCount = sLayer.Features.Count;
                    Int32 sFieldIndex = sLayer.AttributeFields.FindField(sRenderer.Field);
                    MyMapObjects.moValueTypeConstant sValueType = sLayer.AttributeFields.GetItem(sFieldIndex).ValueType;
                    if (sValueType == MyMapObjects.moValueTypeConstant.dText)
                    {
                        MessageBox.Show("该字段不是数值字段，不支持分级渲染！");
                        return;
                    }
                    try
                    {
                        for (Int32 i = 0; i < sFeatrueCount; i++)
                        {
                            double sValue = Convert.ToDouble(sLayer.Features.GetItem(i).Attributes.GetItem(sFieldIndex));
                            sValues.Add(sValue);
                        }
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("该字段不是数值字段，不支持分级渲染！");
                        return;
                    }

                    double sMinValue = sValues.Min();
                    double sMaxValue = sValues.Max();
                    for (Int32 i = 0; i < mPolylineClassBreaksNum; i++)
                    {
                        double sValue = sMinValue + (sMaxValue - sMinValue) * (i + 1) / mPolylineClassBreaksNum;
                        MyMapObjects.moSimpleLineSymbol sSymbol = new MyMapObjects.moSimpleLineSymbol();
                        sSymbol.Color = mPolylineClassBreaksRendererColor;
                        sSymbol.Style = (MyMapObjects.moSimpleLineSymbolStyleConstant)mPolylineSymbolStyle;
                        sRenderer.AddBreakValue(sValue, sSymbol);
                    }
                    double sMinSize = mPolylineClassBreaksRendererMinSize;
                    double sMaxSize = mPolylineClassBreaksRendererMaxSize;
                    sRenderer.RampSize(sMinSize, sMaxSize);
                    sRenderer.DefaultSymbol = new MyMapObjects.moSimpleLineSymbol();
                    sLayer.Renderer = sRenderer;
                    moMap.RedrawMap();
                }
            }
            else if (sLayer.ShapeType == MyMapObjects.moGeometryTypeConstant.MultiPolygon)
            {
                mPolygonRenderer = new PolygonRenderer(moMap.Layers.GetItem(mLastOpLayerIndex));
                mPolygonRenderer.Owner = this;
                mPolygonRenderer.ShowDialog();
                if (mIsInRenderer == false)
                {
                    return;
                }
                //简单渲染
                if (mPolygonRendererMode == 0)
                {
                    MyMapObjects.moSimpleRenderer sRenderer = new MyMapObjects.moSimpleRenderer();
                    MyMapObjects.moSimpleFillSymbol sSymbol = new MyMapObjects.moSimpleFillSymbol();
                    sSymbol.Color = mPolygonSimpleRendererColor;
                    sRenderer.Symbol = sSymbol;
                    sLayer.Renderer = sRenderer;
                    moMap.RedrawMap();
                }
                //唯一值渲染
                else if (mPolygonRendererMode == 1)
                {
                    MyMapObjects.moUniqueValueRenderer sRenderer = new MyMapObjects.moUniqueValueRenderer();
                    sRenderer.Field = sLayer.AttributeFields.GetItem(mPolygonUniqueFieldIndex).Name;
                    List<string> sValues = new List<string>();
                    Int32 sFeatrueCount = sLayer.Features.Count;
                    for (Int32 i = 0; i < sFeatrueCount; i++)
                    {
                        string sValue = Convert.ToString(sLayer.Features.GetItem(i).Attributes.GetItem(mPolygonUniqueFieldIndex));
                        sValues.Add(sValue);
                    }
                    //去除重复
                    sValues = sValues.Distinct().ToList();
                    //生成符号
                    Int32 sValueCount = sValues.Count;
                    for (Int32 i = 0; i <= sValueCount - 1; i++)
                    {
                        MyMapObjects.moSimpleFillSymbol sSymbol = new MyMapObjects.moSimpleFillSymbol();
                        sRenderer.AddUniqueValue(sValues[i], sSymbol);
                    }
                    sRenderer.DefaultSymbol = new MyMapObjects.moSimpleFillSymbol();
                    sLayer.Renderer = sRenderer;
                    moMap.RedrawMap();
                }
                //分级渲染
                else if (mPolygonRendererMode == 2)
                {
                    MyMapObjects.moClassBreaksRenderer sRenderer = new MyMapObjects.moClassBreaksRenderer();
                    sRenderer.Field = sLayer.AttributeFields.GetItem(mPolygonClassBreaksFieldIndex).Name;
                    List<double> sValues = new List<double>();
                    Int32 sFeatrueCount = sLayer.Features.Count;
                    Int32 sFieldIndex = sLayer.AttributeFields.FindField(sRenderer.Field);
                    MyMapObjects.moValueTypeConstant sValueType = sLayer.AttributeFields.GetItem(sFieldIndex).ValueType;
                    if (sValueType == MyMapObjects.moValueTypeConstant.dText)
                    {
                        MessageBox.Show("该字段不是数值字段，不支持分级渲染！");
                        return;
                    }
                    try
                    {
                        for (Int32 i = 0; i < sFeatrueCount; i++)
                        {
                            double sValue = Convert.ToDouble(sLayer.Features.GetItem(i).Attributes.GetItem(sFieldIndex));
                            sValues.Add(sValue);
                        }
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("该字段不是数值字段，不支持分级渲染！");
                        return;
                    }
                    //获取最小最大值并分5级
                    double sMinValue = sValues.Min();
                    double sMaxValue = sValues.Max();
                    for (Int32 i = 0; i < mPolygonClassBreaksNum; i++)
                    {
                        double sValue = sMinValue + (sMaxValue - sMinValue) * (i + 1) / mPolygonClassBreaksNum;
                        MyMapObjects.moSimpleFillSymbol sSymbol = new MyMapObjects.moSimpleFillSymbol();
                        sRenderer.AddBreakValue(sValue, sSymbol);
                    }
                    Color sStartColor = mPolygonClassBreaksRendererStartColor;
                    Color sEndColor = mPolygonClassBreaksRendererEndColor;
                    sRenderer.RampColor(sStartColor, sEndColor);
                    sRenderer.DefaultSymbol = new MyMapObjects.moSimpleFillSymbol();
                    sLayer.Renderer = sRenderer;
                    moMap.RedrawMap();
                }
            }
        }

        private void 显示注记ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MyMapObjects.moMapLayer sLayer = moMap.Layers.GetItem(mLastOpLayerIndex);//待显示注记的图层
            mLabel = new Label(sLayer);
            mLabel.Owner = this;
            mLabel.ShowDialog();
            MyMapObjects.moLabelRenderer sLabelRenderer = new MyMapObjects.moLabelRenderer();
            sLabelRenderer.Field = sLayer.AttributeFields.GetItem(mLabelFieldIndex).Name;
            sLabelRenderer.TextSymbol.Font = mLabelFont;
            sLabelRenderer.TextSymbol.FontColor = mLabelColor;
            sLabelRenderer.TextSymbol.UseMask = mLabelUseMask;
            sLabelRenderer.LabelFeatures = mLabelVisible;
            sLayer.LabelRenderer = sLabelRenderer;
            moMap.RedrawMap();
        }

        public void GetLabel(bool visible, bool useMask, Int32 fieldIndex, Color color, Font font)
        {
            mLabelVisible = visible;
            mLabelUseMask = useMask;
            mLabelFieldIndex = fieldIndex;
            mLabelColor = color;
            mLabelFont = font;
        }

        #endregion

        #region 图层渲染相关
        public void GetPointRenderer(Int32 renderMode, Int32 symbolStyle, Color simpleRendererColor, Double simpleRendererSize,
            Int32 uniqueFieldIndex, Double uniqueRendererSize, Int32 classBreakFieldIndex, Int32 classNum,
            Color classBreakRendererColor, double classBreakRendererMinSize, double classBreakRendererMaxSize)
        {
            mPointRendererMode = renderMode;
            mPointSymbolStyle = symbolStyle;
            mPointSimpleRendererColor = simpleRendererColor;
            mPointSimpleRendererSize = simpleRendererSize;
            mPointUniqueFieldIndex = uniqueFieldIndex;
            mPointUniqueRendererSize = uniqueRendererSize;
            mPointClassBreaksFieldIndex = classBreakFieldIndex;
            mPointClassBreaksNum = classNum;
            mPointClassBreaksRendererColor = classBreakRendererColor;
            mPointClassBreaksRendererMinSize = classBreakRendererMinSize;
            mPointClassBreaksRendererMaxSize = classBreakRendererMaxSize;
            mIsInRenderer = true;
        }

        public void GetPolylineRenderer(Int32 renderMode, Int32 symbolStyle, Color simpleRendererColor, Double simpleRendererSize,
            Int32 uniqueFieldIndex, Double uniqueRendererSize, Int32 classBreakFieldIndex, Int32 classNum,
            Color classBreakRendererColor, double classBreakRendererMinSize, double classBreakRendererMaxSize)
        {
            mPolylineRendererMode = renderMode;
            mPolylineSymbolStyle = symbolStyle;
            mPolylineSimpleRendererColor = simpleRendererColor;
            mPolylineSimpleRendererSize = simpleRendererSize;
            mPolylineUniqueFieldIndex = uniqueFieldIndex;
            mPolylineUniqueRendererSize = uniqueRendererSize;
            mPolylineClassBreaksFieldIndex = classBreakFieldIndex;
            mPolylineClassBreaksNum = classNum;
            mPolylineClassBreaksRendererColor = classBreakRendererColor;
            mPolylineClassBreaksRendererMinSize = classBreakRendererMinSize;
            mPolylineClassBreaksRendererMaxSize = classBreakRendererMaxSize;
            mIsInRenderer = true;
        }

        public void GetPolygonRenderer(Int32 renderMode, Color simpleRendererColor,
            Int32 uniqueFieldIndex, Int32 classBreakFieldIndex, Int32 classNum,
            Color classBreakRendererStartColor, Color classBreakRendererEndColor)
        {
            mPolygonRendererMode = renderMode;
            mPolygonSimpleRendererColor = simpleRendererColor;
            mPolygonUniqueFieldIndex = uniqueFieldIndex;
            mPolygonClassBreaksFieldIndex = classBreakFieldIndex;
            mPolygonClassBreaksNum = classNum;
            mPolygonClassBreaksRendererStartColor = classBreakRendererStartColor;
            mPolygonClassBreaksRendererEndColor = classBreakRendererEndColor;
            mIsInRenderer = true;
        }

        #endregion

        #region 私有函数

        //初始化控件
        private void GvToolsInitial()
        {
            //moMapRightMenu.Items.Add("test");
        }

        // 初始化符号
        private void InitializeSymbols()
        {
            mSelectBoxSymbol = new MyMapObjects.moSimpleFillSymbol();
            mSelectBoxSymbol.Color = Color.Transparent;
            mSelectBoxSymbol.Outline.Color = mSelectBoxColor;
            mSelectBoxSymbol.Outline.Size = mSelectBoxWidth;
            mZoomBoxSymbol = new MyMapObjects.moSimpleFillSymbol();
            mZoomBoxSymbol.Color = Color.Transparent;
            mZoomBoxSymbol.Outline.Color = mZoomBoxColor;
            mZoomBoxSymbol.Outline.Size = mZoomBoxWidth;
            mMovingPolygonSymbol = new MyMapObjects.moSimpleFillSymbol();
            mMovingPolygonSymbol.Color = Color.Transparent;
            mMovingPolygonSymbol.Outline.Color = Color.Black;
            mMovingPolylineSymbol = new MyMapObjects.moSimpleLineSymbol();
            mMovingPolylineSymbol.Color = Color.Black;
            mMovingPointSymbol = new MyMapObjects.moSimpleMarkerSymbol();
            mMovingPointSymbol.Color = Color.Black;
            mEditingPolygonSymbol = new MyMapObjects.moSimpleFillSymbol();
            mEditingPolygonSymbol.Color = Color.Transparent;
            mEditingPolygonSymbol.Outline.Color = Color.DarkGreen;
            mEditingPolygonSymbol.Outline.Size = 0.53;
            mEditingPolylineSymbol = new MyMapObjects.moSimpleLineSymbol();
            mEditingPolylineSymbol.Color = Color.DarkGreen;
            mEditingPolylineSymbol.Size = 0.53;
            mEditingVertexSymbol = new MyMapObjects.moSimpleMarkerSymbol();
            mEditingVertexSymbol.Color = Color.DarkGreen;
            mEditingVertexSymbol.Style = MyMapObjects.moSimpleMarkerSymbolStyleConstant.SolidSquare;
            mEditingVertexSymbol.Size = 2;
            mElasticSymbol = new MyMapObjects.moSimpleLineSymbol();
            mElasticSymbol.Color = Color.DarkGreen;
            mElasticSymbol.Size = 0.52;
            mElasticSymbol.Style = MyMapObjects.moSimpleLineSymbolStyleConstant.Dash;
        }

        // 初始化描绘图形
        private void InitializeSketchingShape()
        {
            mSketchingPoint = new List<MyMapObjects.moPoint>();
            mSketchingShape = new List<MyMapObjects.moPoints>();
            MyMapObjects.moPoints sPoints = new MyMapObjects.moPoints();
            mSketchingShape.Add(sPoints);
        }

        // 根据屏幕坐标显示地图坐标
        private void ShowCoordinate(PointF point)
        {
            MyMapObjects.moPoint sPoint = moMap.ToMapPoint(point.X, point.Y);
            if (mShowLngLat == false)
            {
                double sX = Math.Round(sPoint.X);
                double sY = Math.Round(sPoint.Y);
                tssCoordinate.Text = "X:" + sX.ToString() + ",Y:" + sY.ToString();
            }
            else
            {
                MyMapObjects.moPoint sLngLat = moMap.ProjectionCS.TransferToLngLat(sPoint);
                double sX = Math.Round(sLngLat.X, 4);
                double sY = Math.Round(sLngLat.Y, 4);
                string lng = "°E";
                string lat = "°N";
                if (sX < 0) lng = "°W";
                if (sY < 0) lat = "°S";
                tssCoordinate.Text = sX.ToString() + lng + "," + sY.ToString() + lat;
            }
        }

        //显示当前比例尺
        private void ShowMapScale()
        {
            tssMapScale.Text = "1:" + moMap.MapScale.ToString("0.00");
        }

        //图层列表刷新
        private void RefreshLayersTree()
        {
            layersTree.Nodes.Clear();
            for (Int32 i = 0; i < moMap.Layers.Count; i++)
            {
                TreeNode layerNode = new TreeNode();
                layerNode.Text = moMap.Layers.GetItem(i).Name;
                layerNode.Checked = moMap.Layers.GetItem(i).Visible;
                layerNode.ContextMenuStrip = LayerRightMenu;
                layersTree.Nodes.Add(layerNode);
            }
            layersTree.Refresh();
            mMapOpStyle = 0;
            if(mOperatingLayerIndex != -1)
                layersTree.Nodes[mOperatingLayerIndex].BackColor = Color.LightGray;
        }

        //刷新图层下拉框
        private void RefreshSelectLayer()
        {
            SelectLayer.Items.Clear();
            mLayerIndex.Clear();
            for (Int32 i = 0; i < moMap.Layers.Count; i++)
            {
                if (moMap.Layers.GetItem(i).Visible == true)
                {
                    SelectLayer.Items.Add(moMap.Layers.GetItem(i).Name);
                    mLayerIndex.Add(i);
                }
            }
            Int32 tempIndex = -1;
            for (Int32 i = 0; i < mLayerIndex.Count; i++)
            {
                if (mLayerIndex[i] == mLastOpLayerIndex)
                {
                    tempIndex = i;
                    break;
                }
            }
            if (tempIndex == -1 && mLayerIndex.Count > 0) tempIndex = 0;
            SelectLayer.SelectedIndex = tempIndex;
        }

        //寻找新打开图层的插入位置
        public Int32 SearchInsertIndex(MyMapObjects.moGeometryTypeConstant shapeType)
        {
            Int32 index = 0;
            for (; index < moMap.Layers.Count; index++)
            {
                if (shapeType < moMap.Layers.GetItem(index).ShapeType)
                {
                    return index;
                }
            }
            return index;
        }

        //根据屏幕上两点获得一个地图坐标下的矩形
        private MyMapObjects.moRectangle GetMapRectByTwoPoints(PointF point1, PointF point2)
        {
            MyMapObjects.moPoint sPoint1 = moMap.ToMapPoint(point1.X, point1.Y);
            MyMapObjects.moPoint sPoint2 = moMap.ToMapPoint(point2.X, point2.Y);
            double sMinX = Math.Min(sPoint1.X, sPoint2.X);
            double sMaxX = Math.Max(sPoint1.X, sPoint2.X);
            double sMinY = Math.Min(sPoint1.Y, sPoint2.Y);
            double sMaxY = Math.Max(sPoint1.Y, sPoint2.Y);
            MyMapObjects.moRectangle sRect = new MyMapObjects.moRectangle(sMinX, sMaxX, sMinY, sMaxY);
            return sRect;
        }

        //根据指定的平移量修改移动图形的坐标
        private void ModifyMovingGeometries(double deltaX, double deltaY)
        {
            Int32 sCount = mMovingGeometries.Count;
            for (Int32 i = 0; i <= sCount - 1; i++)
            {
                if (mMovingGeometries[i].GetType() == typeof(MyMapObjects.moMultiPolygon))
                {
                    MyMapObjects.moMultiPolygon sMultiPolygon = (MyMapObjects.moMultiPolygon)mMovingGeometries[i];
                    Int32 sPartCount = sMultiPolygon.Parts.Count;
                    for (Int32 j = 0; j <= sPartCount - 1; j++)
                    {
                        MyMapObjects.moPoints sPoints = sMultiPolygon.Parts.GetItem(j);
                        Int32 sPointCount = sPoints.Count;
                        for (Int32 k = 0; k <= sPointCount - 1; k++)
                        {
                            MyMapObjects.moPoint sPoint = sPoints.GetItem(k);
                            sPoint.X = sPoint.X + deltaX;
                            sPoint.Y = sPoint.Y + deltaY;
                        }
                    }
                    sMultiPolygon.UpdateExtent();
                }
                else if (mMovingGeometries[i].GetType() == typeof(MyMapObjects.moMultiPolyline))
                {
                    MyMapObjects.moMultiPolyline sMultiPolyline = (MyMapObjects.moMultiPolyline)mMovingGeometries[i];
                    Int32 sPartCount = sMultiPolyline.Parts.Count;
                    for (Int32 j = 0; j <= sPartCount - 1; j++)
                    {
                        MyMapObjects.moPoints sPoints = sMultiPolyline.Parts.GetItem(j);
                        Int32 sPointCount = sPoints.Count;
                        for (Int32 k = 0; k <= sPointCount - 1; k++)
                        {
                            MyMapObjects.moPoint sPoint = sPoints.GetItem(k);
                            sPoint.X = sPoint.X + deltaX;
                            sPoint.Y = sPoint.Y + deltaY;
                        }
                    }
                    sMultiPolyline.UpdateExtent();
                }
                else if (mMovingGeometries[i].GetType() == typeof(MyMapObjects.moPoint))
                {
                    MyMapObjects.moPoint sPoint = (MyMapObjects.moPoint)mMovingGeometries[i];
                    sPoint.X = sPoint.X + deltaX;
                    sPoint.Y = sPoint.Y + deltaY;
                }
                else if (mMovingGeometries[i].GetType() == typeof(MyMapObjects.moPoints))
                {
                    MyMapObjects.moPoints sPoints = (MyMapObjects.moPoints)mMovingGeometries[i];
                    for (Int32 j = 0; j < sPoints.Count; j++)
                    {
                        MyMapObjects.moPoint sPoint = sPoints.GetItem(j);
                        sPoint.X = sPoint.X + deltaX;
                        sPoint.Y = sPoint.Y + deltaY;
                    }
                    sPoints.UpdateExtent();
                }
            }
        }

        //绘制正在移动的图形
        private void DrawMovingShape()
        {
            MyMapObjects.moUserDrawingTool sDrawingTool = moMap.GetDrawingTool();
            Int32 sCount = mMovingGeometries.Count;
            for (Int32 i = 0; i <= sCount - 1; i++)
            {
                if (mMovingGeometries[i].GetType() == typeof(MyMapObjects.moMultiPolygon))
                {
                    MyMapObjects.moMultiPolygon sMultiPolygon = (MyMapObjects.moMultiPolygon)mMovingGeometries[i];
                    sDrawingTool.DrawMultiPolygon(sMultiPolygon, mMovingPolygonSymbol);
                }
                else if (mMovingGeometries[i].GetType() == typeof(MyMapObjects.moMultiPolyline))
                {
                    MyMapObjects.moMultiPolyline sMultiPolyline = (MyMapObjects.moMultiPolyline)mMovingGeometries[i];
                    sDrawingTool.DrawMultiPolyline(sMultiPolyline, mMovingPolylineSymbol);
                }
                else if (mMovingGeometries[i].GetType() == typeof(MyMapObjects.moPoint))
                {
                    MyMapObjects.moPoint sPoint = (MyMapObjects.moPoint)mMovingGeometries[i];
                    sDrawingTool.DrawPoint(sPoint, mMovingPointSymbol);
                }
                else if (mMovingGeometries[i].GetType() == typeof(MyMapObjects.moPoints))
                {
                    MyMapObjects.moPoints sPoints = (MyMapObjects.moPoints)mMovingGeometries[i];
                    sDrawingTool.DrawPoints(sPoints, mMovingPointSymbol);
                }
            }
        }

        //取消修改
        private void CancelEdit()
        {
            try
            {
                if (mPointEditNeedSave)
                {
                    mEditingGeometry = null;
                    mPointEditNeedSave = false;
                    moMap.RedrawMap();
                }
                for (Int32 i = 0; i < moMap.Layers.Count; i++)
                {
                    MyMapObjects.moMapLayer sLayer = moMap.Layers.GetItem(i);
                    MyMapObjects.moMapLayer sMapLayer = new MyMapObjects.moMapLayer(sLayer.Name, sLayer.ShapeType, mDbfFiles[i].Fields);
                    //加载要素
                    MyMapObjects.moFeatures sFeatures = new MyMapObjects.moFeatures();
                    for (Int32 j = 0; j < mGvShapeFiles[i].Geometries.Count; ++j)
                    {
                        MyMapObjects.moFeature sFeature = new MyMapObjects.moFeature(mGvShapeFiles[i].GeometryType,
                            mGvShapeFiles[i].Geometries[j], mDbfFiles[i].AttributesList[j]);
                        sFeatures.Add(sFeature);
                    }
                    sMapLayer.Features = sFeatures;
                    sMapLayer.Renderer = sLayer.Renderer;
                    sMapLayer.UpdateExtent();
                    moMap.Layers.RemoveAt(i);
                    moMap.Layers.Insert(i, sMapLayer);
                }
                mNeedToSave = false;
            }
            catch (Exception error)
            {
                MessageBox.Show(error.ToString());
                return;
            }
        }

        //结束描绘部件
        private void EndSketchPart(MyMapObjects.moGeometryTypeConstant shapeType)
        {
            mNeedToSave = true;
            if (shapeType == MyMapObjects.moGeometryTypeConstant.MultiPolygon)
            {
                //判断是否可以结束，即是否最少三个点
                if (mSketchingShape.Last().Count < 3)
                    return;
                //往描绘图形中增加一个多点对象
                MyMapObjects.moPoints sPoints = new MyMapObjects.moPoints();
                mSketchingShape.Add(sPoints);
                //重绘跟踪层
                moMap.RedrawTrackingShapes();
            }
            else if (shapeType == MyMapObjects.moGeometryTypeConstant.MultiPolyline)
            {
                //判断是否可以结束，即是否最少两个点
                if (mSketchingShape.Last().Count < 2)
                    return;
                //往描绘图形中增加一个多点对象
                MyMapObjects.moPoints sPoints = new MyMapObjects.moPoints();
                mSketchingShape.Add(sPoints);
                //重绘跟踪层
                moMap.RedrawTrackingShapes();
            }
            else if (shapeType == MyMapObjects.moGeometryTypeConstant.Point)
            {
                ;
            }
            else if (shapeType == MyMapObjects.moGeometryTypeConstant.MultiPoint)
            {
                //重绘跟踪层
                moMap.RedrawTrackingShapes();
            }
        }

        //结束描绘图形
        private void EndSketchGeo(MyMapObjects.moGeometryTypeConstant shapeType)
        {
            mNeedToSave = true;
            if (shapeType == MyMapObjects.moGeometryTypeConstant.MultiPolygon)
            {
                if (mSketchingShape.Last().Count >= 1 && mSketchingShape.Last().Count < 3)
                    return;
                //如果最后一个部件的点数为0，则删除最后一个部件
                if (mSketchingShape.Last().Count == 0)
                {
                    mSketchingShape.Remove(mSketchingShape.Last());
                }
                //如果用户的确输入了，则加入多边形图层
                if (mSketchingShape.Count > 0)
                {
                    //查找多边形图层，如果有则加入该图层
                    MyMapObjects.moMapLayer sLayer = moMap.Layers.GetItem(mOperatingLayerIndex);
                    //新建复合多边形
                    MyMapObjects.moMultiPolygon sMultiPolygon = new MyMapObjects.moMultiPolygon();
                    sMultiPolygon.Parts.AddRange(mSketchingShape.ToArray());
                    sMultiPolygon.UpdateExtent();
                    //生成要素并加入图层
                    MyMapObjects.moFeature sFeature = sLayer.GetNewFeature();
                    sFeature.Geometry = sMultiPolygon;
                    sLayer.Features.Add(sFeature);
                    sLayer.UpdateExtent();
                    sLayer.SelectedFeatures.Clear();
                    sLayer.SelectedFeatures.Add(sFeature);
                }
                //初始化描绘图形
                InitializeSketchingShape();
                //重绘地图
                moMap.RedrawMap();
            }
            else if (shapeType == MyMapObjects.moGeometryTypeConstant.MultiPolyline)
            {
                if (mSketchingShape.Last().Count == 1)
                    return;
                //如果最后一个部件的点数为0，则删除最后一个部件
                if (mSketchingShape.Last().Count == 0)
                {
                    mSketchingShape.Remove(mSketchingShape.Last());
                }
                //如果用户的确输入了，则加入多边形图层
                if (mSketchingShape.Count > 0)
                {
                    //查找多边形图层，如果有则加入该图层
                    MyMapObjects.moMapLayer sLayer = moMap.Layers.GetItem(mOperatingLayerIndex);
                    //新建复合多边形
                    MyMapObjects.moMultiPolyline sMultiPolyline = new MyMapObjects.moMultiPolyline();
                    sMultiPolyline.Parts.AddRange(mSketchingShape.ToArray());
                    sMultiPolyline.UpdateExtent();
                    //生成要素并加入图层
                    MyMapObjects.moFeature sFeature = sLayer.GetNewFeature();
                    sFeature.Geometry = sMultiPolyline;
                    sLayer.Features.Add(sFeature);
                    sLayer.UpdateExtent();
                    sLayer.SelectedFeatures.Clear();
                    sLayer.SelectedFeatures.Add(sFeature);
                }
                //初始化描绘图形
                InitializeSketchingShape();
                //重绘地图
                moMap.RedrawMap();
            }
            else if (shapeType == MyMapObjects.moGeometryTypeConstant.Point)
            {
                if (mSketchingPoint.Count == 1)
                {
                    MyMapObjects.moMapLayer sLayer = moMap.Layers.GetItem(mOperatingLayerIndex);
                    MyMapObjects.moFeature sFeature = sLayer.GetNewFeature();
                    sFeature.Geometry = mSketchingPoint[0];
                    sLayer.Features.Add(sFeature);
                    sLayer.UpdateExtent();
                    sLayer.SelectedFeatures.Clear();
                    sLayer.SelectedFeatures.Add(sFeature);
                }
                InitializeSketchingShape();
                moMap.RedrawMap();
            }
            else if (shapeType == MyMapObjects.moGeometryTypeConstant.MultiPoint)
            {
                if (mSketchingPoint.Count > 0)
                {
                    MyMapObjects.moMapLayer sLayer = moMap.Layers.GetItem(mOperatingLayerIndex);
                    //新建复合多边形
                    MyMapObjects.moPoints sPoints = new MyMapObjects.moPoints();
                    sPoints.AddRange(mSketchingPoint.ToArray());
                    sPoints.UpdateExtent();
                    //生成要素并加入图层
                    MyMapObjects.moFeature sFeature = sLayer.GetNewFeature();
                    sFeature.Geometry = sPoints;
                    sLayer.Features.Add(sFeature);
                    sLayer.UpdateExtent();
                    sLayer.SelectedFeatures.Clear();
                    sLayer.SelectedFeatures.Add(sFeature);
                }
                //初始化描绘图形
                InitializeSketchingShape();
                //重绘地图
                moMap.RedrawMap();
            }
        }

        //删除上一个节点
        private void DeleteLastSketchPoint(MyMapObjects.moGeometryTypeConstant shapeType)
        {
            mNeedToSave = true;
            if (shapeType == MyMapObjects.moGeometryTypeConstant.MultiPolygon)
            {
                Int32 pointsNum = mSketchingShape.Last().Count;
                if (pointsNum == 0)
                {
                    Int32 partsNum = mSketchingShape.Count;
                    if (partsNum > 1)
                    {
                        mSketchingShape.RemoveAt(partsNum - 1);
                        Int32 newPointsNum = mSketchingShape.Last().Count;
                        mSketchingShape.Last().RemoveAt(newPointsNum - 1);
                    }
                    else
                    {
                        return;
                    }
                }
                else mSketchingShape.Last().RemoveAt(pointsNum - 1);
                moMap.RedrawTrackingShapes();
            }
            else if (shapeType == MyMapObjects.moGeometryTypeConstant.MultiPolyline)
            {
                Int32 pointsNum = mSketchingShape.Last().Count;
                if (pointsNum == 0)
                {
                    Int32 partsNum = mSketchingShape.Count;
                    if (partsNum > 1)
                    {
                        mSketchingShape.RemoveAt(partsNum - 1);
                        Int32 newPointsNum = mSketchingShape.Last().Count;
                        mSketchingShape.Last().RemoveAt(newPointsNum - 1);
                    }
                    else
                    {
                        return;
                    }
                }
                else mSketchingShape.Last().RemoveAt(pointsNum - 1);
                moMap.RedrawTrackingShapes();
            }
            else if (shapeType == MyMapObjects.moGeometryTypeConstant.Point)
            {
                ;
            }
            else if (shapeType == MyMapObjects.moGeometryTypeConstant.MultiPoint)
            {
                Int32 pointsNum = mSketchingPoint.Count;
                if (pointsNum == 0) return;
                mSketchingPoint.RemoveAt(pointsNum - 1);
                moMap.RedrawTrackingShapes();
            }
        }

        //撤销上一个部件
        private void DeleteLastSketchPart(MyMapObjects.moGeometryTypeConstant shapeType)
        {
            mNeedToSave = true;
            if (shapeType == MyMapObjects.moGeometryTypeConstant.MultiPolygon)
            {
                Int32 pointsNum = mSketchingShape.Last().Count;
                Int32 partsNum = mSketchingShape.Count;
                if (partsNum == 1) InitializeSketchingShape();
                else
                {
                    if (pointsNum == 0)
                    {
                        if (partsNum == 2) InitializeSketchingShape();
                        else mSketchingShape.RemoveAt(partsNum - 2);
                    }
                    else
                    {
                        mSketchingShape.RemoveAt(partsNum - 1);
                        mSketchingShape.Add(new MyMapObjects.moPoints());
                    }
                }
                moMap.RedrawTrackingShapes();
            }
            else if (shapeType == MyMapObjects.moGeometryTypeConstant.MultiPolyline)
            {
                Int32 pointsNum = mSketchingShape.Last().Count;
                Int32 partsNum = mSketchingShape.Count;
                if (partsNum == 1) InitializeSketchingShape();
                else
                {
                    if (pointsNum == 0)
                    {
                        if (partsNum == 2) InitializeSketchingShape();
                        else mSketchingShape.RemoveAt(partsNum - 1);
                    }
                    else mSketchingShape.RemoveAt(partsNum - 1);
                }
                moMap.RedrawTrackingShapes();
            }
            else if (shapeType == MyMapObjects.moGeometryTypeConstant.Point)
            {
                ;
            }
            else if (shapeType == MyMapObjects.moGeometryTypeConstant.MultiPoint)
            {
                Int32 pointsNum = mSketchingPoint.Count;
                if (pointsNum == 0) return;
                mSketchingPoint.RemoveAt(pointsNum - 1);
                moMap.RedrawTrackingShapes();
            }
        }

        //撤销上一个草图
        private void DeleteLastSketchGeo()
        {
            mNeedToSave = true;
            InitializeSketchingShape();
            moMap.RedrawTrackingShapes();
        }

        //绘制正在描绘的图形
        private void DrawSketchingShapes(MyMapObjects.moUserDrawingTool drawingTool)
        {
            if (mOperatingLayerIndex == -1) return;
            MyMapObjects.moMapLayer sLayer = moMap.Layers.GetItem(mOperatingLayerIndex);
            if (sLayer.ShapeType == MyMapObjects.moGeometryTypeConstant.MultiPolygon)
            {
                if (mSketchingShape == null)
                    return;
                Int32 sPartCount = mSketchingShape.Count;
                //绘制已经描绘完成的部分
                for (Int32 i = 0; i <= sPartCount - 2; i++)
                {
                    drawingTool.DrawPolygon(mSketchingShape[i], mEditingPolygonSymbol);
                }
                //正在描绘的部分（只有一个Part）
                MyMapObjects.moPoints sLastPart = mSketchingShape.Last();
                if (sLastPart.Count >= 2)
                    drawingTool.DrawPolyline(sLastPart, mEditingPolygonSymbol.Outline);
                //绘制所有顶点手柄
                for (Int32 i = 0; i <= sPartCount - 1; i++)
                {
                    MyMapObjects.moPoints sPoints = mSketchingShape[i];
                    drawingTool.DrawPoints(sPoints, mEditingVertexSymbol);
                }
            }
            else if (sLayer.ShapeType == MyMapObjects.moGeometryTypeConstant.MultiPolyline)
            {
                if (mSketchingShape == null)
                    return;
                Int32 sPartCount = mSketchingShape.Count;
                //绘制已经描绘完成的部分
                for (Int32 i = 0; i <= sPartCount - 2; i++)
                {
                    drawingTool.DrawPolyline(mSketchingShape[i], mEditingPolylineSymbol);
                }
                //正在描绘的部分（只有一个Part）
                MyMapObjects.moPoints sLastPart = mSketchingShape.Last();
                if (sLastPart.Count >= 2)
                {
                    drawingTool.DrawPolyline(sLastPart, mEditingPolylineSymbol);
                }
                //绘制所有顶点手柄
                for (Int32 i = 0; i <= sPartCount - 1; i++)
                {
                    MyMapObjects.moPoints sPoints = mSketchingShape[i];
                    drawingTool.DrawPoints(sPoints, mEditingVertexSymbol);
                }
            }
            else if (sLayer.ShapeType == MyMapObjects.moGeometryTypeConstant.Point)
            {
                if (mSketchingPoint == null || mSketchingPoint.Count == 0)
                    return;
                drawingTool.DrawPoint(mSketchingPoint[0], mEditingVertexSymbol);
            }
            else if (sLayer.ShapeType == MyMapObjects.moGeometryTypeConstant.MultiPoint)
            {
                if (mSketchingPoint == null || mSketchingPoint.Count == 0)
                    return;
                MyMapObjects.moPoints sPoints = new MyMapObjects.moPoints();
                sPoints.AddRange(mSketchingPoint.ToArray());
                drawingTool.DrawPoints(sPoints, mEditingVertexSymbol);
            }
        }

        //绘制正在编辑的图形
        private void DrawEditingShapes(MyMapObjects.moUserDrawingTool drawingTool)
        {
            if (mEditingGeometry == null)
                return;
            if (mEditingGeometry.GetType() == typeof(MyMapObjects.moMultiPolygon))
            {
                MyMapObjects.moMultiPolygon sMultiPolygon = (MyMapObjects.moMultiPolygon)mEditingGeometry;
                //绘制边界
                drawingTool.DrawMultiPolygon(sMultiPolygon, mEditingPolygonSymbol);
                //绘制顶点手柄
                Int32 sPartCount = sMultiPolygon.Parts.Count;
                for (Int32 i = 0; i <= sPartCount - 1; i++)
                {
                    MyMapObjects.moPoints sPoints = sMultiPolygon.Parts.GetItem(i);
                    drawingTool.DrawPoints(sPoints, mEditingVertexSymbol);
                }
            }
            else if (mEditingGeometry.GetType() == typeof(MyMapObjects.moMultiPolyline))
            {
                MyMapObjects.moMultiPolyline sMultiPolyline = (MyMapObjects.moMultiPolyline)mEditingGeometry;
                drawingTool.DrawMultiPolyline(sMultiPolyline, mEditingPolylineSymbol);
                Int32 sPartCount = sMultiPolyline.Parts.Count;
                for (Int32 i = 0; i <= sPartCount - 1; i++)
                {
                    MyMapObjects.moPoints sPoints = sMultiPolyline.Parts.GetItem(i);
                    drawingTool.DrawPoints(sPoints, mEditingVertexSymbol);
                }
            }
            else if (mEditingGeometry.GetType() == typeof(MyMapObjects.moPoint))
            {
                MyMapObjects.moPoint sPoint = (MyMapObjects.moPoint)mEditingGeometry;
                drawingTool.DrawPoint(sPoint, mEditingVertexSymbol);
            }
            else if (mEditingGeometry.GetType() == typeof(MyMapObjects.moPoints))
            {
                MyMapObjects.moPoints sPoints = (MyMapObjects.moPoints)mEditingGeometry;
                drawingTool.DrawPoints(sPoints, mEditingVertexSymbol);
            }
        }

        //选择状态右键菜单
        private void RightMenuInSelect()
        {
            if (mOperatingLayerIndex == -1) return;
            moMapRightMenu.Items.Clear();
            moMapRightMenu.Items.Add("复制");
            moMapRightMenu.Items.Add("粘贴");
            moMapRightMenu.Items.Add("删除");
            moMapRightMenu.Items.Add("复制并粘贴到新建图层");
        }

        //描绘状态右键菜单
        private void RightMenuInSketch()
        {
            if (mOperatingLayerIndex == -1) return;
            moMapRightMenu.Items.Clear();
            moMapRightMenu.Items.Add("结束部件");
            moMapRightMenu.Items.Add("完成草图");
            moMapRightMenu.Items.Add("撤销绘制中节点");
            moMapRightMenu.Items.Add("撤销绘制中部件");
            moMapRightMenu.Items.Add("撤销绘制中草图");
        }

        //编辑折点状态右键菜单
        private void RightMenuInEdit()
        {
            if (mOperatingLayerIndex == -1) return;
            moMapRightMenu.Items.Clear();
            moMapRightMenu.Items.Add("完成节点编辑");
            moMapRightMenu.Items.Add("撤销上一步操作");
        }

        //选择状态右键菜单操作
        private void RightOperateInSelect(ToolStripItemClickedEventArgs e)
        {
            if (mOperatingLayerIndex == -1) return;
            MyMapObjects.moMapLayer sLayer = moMap.Layers.GetItem(mOperatingLayerIndex);
            if (e.ClickedItem.Text == "删除")
            {
                mNeedToSave = true;
                sLayer.RemoveSelection();
                moMap.RedrawMap();
            }
            if (e.ClickedItem.Text == "复制")
            {
                CopySelectedFeatures();
            }
            if (e.ClickedItem.Text == "粘贴")
            {
                PasteSelectedFeatures();
            }
            if (e.ClickedItem.Text == "复制并粘贴到新建图层")
            {
                CopySelectedFeatures();
                MyMapObjects.moGeometryTypeConstant shapeType = moMap.Layers.GetItem(mOperatingLayerIndex).ShapeType;
                CreateLayer newLayer = new CreateLayer();
                newLayer.Owner = this;
                newLayer.FixedType(shapeType);
                newLayer.ShowDialog();
            }
        }

        //描绘状态右键菜单操作
        private void RightOperateInSketch(ToolStripItemClickedEventArgs e)
        {
            if (mOperatingLayerIndex == -1) return;
            MyMapObjects.moMapLayer sLayer = moMap.Layers.GetItem(mOperatingLayerIndex);
            if (e.ClickedItem.Text == "结束部件")
            {
                EndSketchPart(sLayer.ShapeType);
            }
            else if (e.ClickedItem.Text == "完成草图")
            {
                EndSketchGeo(sLayer.ShapeType);
            }
            else if (e.ClickedItem.Text == "撤销绘制中节点")
            {
                DeleteLastSketchPoint(sLayer.ShapeType);
            }
            else if (e.ClickedItem.Text == "撤销绘制中部件")
            {
                DeleteLastSketchPart(sLayer.ShapeType);
            }
            else if (e.ClickedItem.Text == "撤销绘制中草图")
            {
                DeleteLastSketchGeo();
            }
        }

        //编辑节点状态下右键菜单操作
        private void RightOperateInEditPoint(ToolStripItemClickedEventArgs e)
        {
            if (mOperatingLayerIndex == -1) return;
            MyMapObjects.moMapLayer sLayer = moMap.Layers.GetItem(mOperatingLayerIndex);
            if (e.ClickedItem.Text == "完成节点编辑")
            {
                MoveFeatureBtn_Click(new object(), e);
            }
            if (e.ClickedItem.Text == "撤销上一步操作")
            {
                Int32 editOp = mEditPointOperation.Last();
                if (editOp == 1)
                {
                    if (sLayer.ShapeType == MyMapObjects.moGeometryTypeConstant.MultiPolygon)
                    {
                        MyMapObjects.moMultiPolygon sMultiPolygon = (MyMapObjects.moMultiPolygon)mEditingGeometry;
                        MyMapObjects.moPoint sPoint = sMultiPolygon.Parts.GetItem(mPastPartIndex.Last()).GetItem(mPastPointIndex.Last());
                        sPoint.X = mEditPointRecord.Last().X;
                        sPoint.Y = mEditPointRecord.Last().Y;
                        sMultiPolygon.UpdateExtent();
                        mEditingGeometry = sMultiPolygon;
                    }
                    else if (sLayer.ShapeType == MyMapObjects.moGeometryTypeConstant.MultiPolyline)
                    {
                        MyMapObjects.moMultiPolyline sMultiPolyline = (MyMapObjects.moMultiPolyline)mEditingGeometry;
                        MyMapObjects.moPoint sPoint = sMultiPolyline.Parts.GetItem(mPastPartIndex.Last()).GetItem(mPastPointIndex.Last());
                        sPoint.X = mEditPointRecord.Last().X;
                        sPoint.Y = mEditPointRecord.Last().Y;
                        sMultiPolyline.UpdateExtent();
                        mEditingGeometry = sMultiPolyline;
                    }
                    else if (sLayer.ShapeType == MyMapObjects.moGeometryTypeConstant.Point)
                    {
                        MyMapObjects.moPoint sPoint = (MyMapObjects.moPoint)mEditingGeometry;
                        sPoint.X = mEditPointRecord.Last().X;
                        sPoint.Y = mEditPointRecord.Last().Y;
                        mEditingGeometry = sPoint;
                    }
                    else if (sLayer.ShapeType == MyMapObjects.moGeometryTypeConstant.MultiPoint)
                    {
                        MyMapObjects.moPoints sPoints = (MyMapObjects.moPoints)mEditingGeometry;
                        MyMapObjects.moPoint sPoint = sPoints.GetItem(mPastPointIndex.Last());
                        sPoint.X = mEditPointRecord.Last().X;
                        sPoint.Y = mEditPointRecord.Last().Y;
                        sPoints.UpdateExtent();
                        mEditingGeometry = sPoints;
                    }
                    mEditPointOperation.RemoveAt(mEditPointOperation.Count - 1);
                    mEditPointRecord.RemoveAt(mEditPointRecord.Count - 1);
                    mPastPartIndex.RemoveAt(mPastPartIndex.Count - 1);
                    mPastPointIndex.RemoveAt(mPastPointIndex.Count - 1);
                    moMap.RedrawTrackingShapes();
                }
                else if (editOp == 2)
                {
                    if (sLayer.ShapeType == MyMapObjects.moGeometryTypeConstant.MultiPolygon)
                    {
                        MyMapObjects.moMultiPolygon sMultiPolygon = (MyMapObjects.moMultiPolygon)mEditingGeometry;
                        sMultiPolygon.Parts.GetItem(mPastPartIndex.Last()).RemoveAt(mPastPointIndex.Last() + 1);
                        sMultiPolygon.UpdateExtent();
                        mEditingGeometry = sMultiPolygon;
                    }
                    else if (sLayer.ShapeType == MyMapObjects.moGeometryTypeConstant.MultiPolyline)
                    {
                        MyMapObjects.moMultiPolyline sMultiPolyline = (MyMapObjects.moMultiPolyline)mEditingGeometry;
                        sMultiPolyline.Parts.GetItem(mPastPartIndex.Last()).RemoveAt(mPastPointIndex.Last() + 1);
                        sMultiPolyline.UpdateExtent();
                        mEditingGeometry = sMultiPolyline;
                    }
                    else if (sLayer.ShapeType == MyMapObjects.moGeometryTypeConstant.Point)
                    {
                        ;
                    }
                    else if (sLayer.ShapeType == MyMapObjects.moGeometryTypeConstant.MultiPoint)
                    {
                        MyMapObjects.moPoints sPoints = (MyMapObjects.moPoints)mEditingGeometry;
                        sPoints.RemoveAt(mPastPointIndex.Last());
                        sPoints.UpdateExtent();
                        mEditingGeometry = sPoints;
                    }
                    mEditPointOperation.RemoveAt(mEditPointOperation.Count - 1);
                    mPastPartIndex.RemoveAt(mPastPartIndex.Count - 1);
                    mPastPointIndex.RemoveAt(mPastPointIndex.Count - 1);
                    moMap.RedrawTrackingShapes();
                }
                else if (editOp == 3)
                {
                    if (sLayer.ShapeType == MyMapObjects.moGeometryTypeConstant.MultiPolygon)
                    {
                        MyMapObjects.moMultiPolygon sMultiPolygon = (MyMapObjects.moMultiPolygon)mEditingGeometry;
                        MyMapObjects.moPoints sPoints = sMultiPolygon.Parts.GetItem(mPastPartIndex.Last());
                        sPoints.Insert(mPastPointIndex.Last(), mEditPointRecord.Last());
                        sMultiPolygon.UpdateExtent();
                        mEditingGeometry = sMultiPolygon;
                    }
                    else if (sLayer.ShapeType == MyMapObjects.moGeometryTypeConstant.MultiPolyline)
                    {
                        MyMapObjects.moMultiPolyline sMultiPolyline = (MyMapObjects.moMultiPolyline)mEditingGeometry;
                        MyMapObjects.moPoints sPoints = sMultiPolyline.Parts.GetItem(mPastPartIndex.Last());
                        sPoints.Insert(mPastPointIndex.Last(), mEditPointRecord.Last());
                        sMultiPolyline.UpdateExtent();
                        mEditingGeometry = sMultiPolyline;
                    }
                    else if (sLayer.ShapeType == MyMapObjects.moGeometryTypeConstant.Point)
                    {
                        ;
                    }
                    else if (sLayer.ShapeType == MyMapObjects.moGeometryTypeConstant.MultiPoint)
                    {
                        MyMapObjects.moPoints sPoints = (MyMapObjects.moPoints)mEditingGeometry;
                        sPoints.Insert(mPastPointIndex.Last(), mEditPointRecord.Last());
                        sPoints.UpdateExtent();
                        mEditingGeometry = sPoints;
                    }
                    mEditPointOperation.RemoveAt(mEditPointOperation.Count - 1);
                    mEditPointRecord.RemoveAt(mEditPointRecord.Count - 1);
                    mPastPartIndex.RemoveAt(mPastPartIndex.Count - 1);
                    mPastPointIndex.RemoveAt(mPastPointIndex.Count - 1);
                    moMap.RedrawTrackingShapes();
                }
            }
        }

        //显示编辑节点工具栏
        private void ShowEditStrip(MyMapObjects.moGeometryTypeConstant shapeType)
        {
            EditPointStrip.Enabled = true;
            EditPointStrip.Visible = true;
            MovePointBtn.Enabled = true;
            MovePointBtn.Visible = true;
            MovePointBtn.Checked = true;
            if (shapeType == MyMapObjects.moGeometryTypeConstant.Point) return;
            AddPointBtn.Enabled = true;
            AddPointBtn.Visible = true;
            DeletePointBtn.Enabled = true;
            DeletePointBtn.Visible = true;
        }

        //隐藏编辑节点工具栏
        private void HideEditStrip()
        {
            EditPointStrip.Enabled = false;
            EditPointStrip.Visible = false;
            MovePointBtn.Enabled = false;
            MovePointBtn.Visible = false;
            MovePointBtn.Checked = false;
            AddPointBtn.Enabled = false;
            AddPointBtn.Visible = false;
            AddPointBtn.Checked = false;
            DeletePointBtn.Enabled = false;
            DeletePointBtn.Visible = false;
            DeletePointBtn.Checked = false;
        }

        //显示编辑节点的图形
        private void ShowEditGeometry()
        {
            if (mOperatingLayerIndex == -1) return;
            MyMapObjects.moMapLayer sLayer = moMap.Layers.GetItem(mOperatingLayerIndex);
            if (sLayer.ShapeType == MyMapObjects.moGeometryTypeConstant.MultiPolygon)
            {
                MyMapObjects.moMultiPolygon sOriMultiPolygon = (MyMapObjects.moMultiPolygon)sLayer.SelectedFeatures.GetItem(0).Geometry;
                MyMapObjects.moMultiPolygon sDesMultiPolygon = sOriMultiPolygon.Clone();
                mEditingGeometry = sDesMultiPolygon;
            }
            else if (sLayer.ShapeType == MyMapObjects.moGeometryTypeConstant.MultiPolyline)
            {
                MyMapObjects.moMultiPolyline sOriMultiPolyline = (MyMapObjects.moMultiPolyline)sLayer.SelectedFeatures.GetItem(0).Geometry;
                MyMapObjects.moMultiPolyline sDesMultiPolyline = sOriMultiPolyline.Clone();
                mEditingGeometry = sDesMultiPolyline;
            }
            else if (sLayer.ShapeType == MyMapObjects.moGeometryTypeConstant.Point)
            {
                MyMapObjects.moPoint sOriPoint = (MyMapObjects.moPoint)sLayer.SelectedFeatures.GetItem(0).Geometry;
                MyMapObjects.moPoint sDesPoint = sOriPoint.Clone();
                mEditingGeometry = sDesPoint;
            }
            else if (sLayer.ShapeType == MyMapObjects.moGeometryTypeConstant.MultiPoint)
            {
                MyMapObjects.moPoints sOriPoints = (MyMapObjects.moPoints)sLayer.SelectedFeatures.GetItem(0).Geometry;
                MyMapObjects.moPoints sDesPoints = sOriPoints.Clone();
                mEditingGeometry = sDesPoints;
            }
            moMap.RedrawTrackingShapes();
        }

        //保存当前对节点的编辑
        private void SavePointEdit()
        {
            if (mPointEditNeedSave)
            {
                MyMapObjects.moMapLayer sLayer = moMap.Layers.GetItem(mOperatingLayerIndex);
                sLayer.SelectedFeatures.GetItem(0).Geometry = mEditingGeometry;
                sLayer.UpdateExtent();
                mEditingGeometry = null;
                mMouseOnPartIndex = -1;
                mMouseOnPointIndex = -1;
                mPointEditNeedSave = false;
                mNeedToSave = true;
                DeleteEditPointRecord();
                moMap.RedrawMap();
            }
            else
            {
                mEditingGeometry = null;
                mMouseOnPartIndex = -1;
                mMouseOnPointIndex = -1;
                moMap.RedrawMap();
            }
            this.Cursor = Cursors.Default;
        }

        //判断点是否靠近多边形
        private bool PointCloseToMultiPolygonPoint(MyMapObjects.moPoint sPoint, MyMapObjects.moMultiPolygon sMultiPolygon, double sTolerance)
        {
            if (mIsInMovePoint || mIsInDeletePoint)
            {
                for (Int32 i = 0; i < sMultiPolygon.Parts.Count; i++)
                {
                    MyMapObjects.moPoints sPoints = sMultiPolygon.Parts.GetItem(i);
                    Int32 j = 0;
                    for (; j < sPoints.Count; j++)
                    {
                        if (MyMapObjects.moMapTools.IsPointOnPoint(sPoint, sPoints.GetItem(j), sTolerance))
                        {
                            mMouseOnPartIndex = i;
                            mMouseOnPointIndex = j;
                            return true;
                        }
                    }
                }
            }
            if (mIsInAddPoint)
            {
                for (Int32 i = 0; i < sMultiPolygon.Parts.Count; i++)
                {
                    MyMapObjects.moPoints sPoints = sMultiPolygon.Parts.GetItem(i);
                    for (Int32 j = 0; j < sPoints.Count; j++)
                    {
                        MyMapObjects.moPoints tempPoints = new MyMapObjects.moPoints();
                        tempPoints.Add(sPoints.GetItem(j));
                        if (j < sPoints.Count - 1)
                        {
                            tempPoints.Add(sPoints.GetItem(j + 1));
                        }
                        else
                        {
                            tempPoints.Add(sPoints.GetItem(0));
                        }
                        tempPoints.UpdateExtent();
                        if (MyMapObjects.moMapTools.IsPointOnPolyline(sPoint, tempPoints, sTolerance))
                        {
                            mMouseOnPartIndex = i;
                            mMouseOnPointIndex = j;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        //判断点是否靠近折线
        private bool PointCloseToMultiPolylinePoint(MyMapObjects.moPoint sPoint, MyMapObjects.moMultiPolyline sMultiPolyline, double sTolerance)
        {
            if (mIsInMovePoint || mIsInDeletePoint)
            {
                for (Int32 i = 0; i < sMultiPolyline.Parts.Count; i++)
                {
                    MyMapObjects.moPoints sPoints = sMultiPolyline.Parts.GetItem(i);
                    Int32 j = 0;
                    for (; j < sPoints.Count; j++)
                    {
                        if (MyMapObjects.moMapTools.IsPointOnPoint(sPoint, sPoints.GetItem(j), sTolerance))
                        {
                            mMouseOnPartIndex = i;
                            mMouseOnPointIndex = j;
                            return true;
                        }
                    }
                }
            }
            if (mIsInAddPoint)
            {
                for (Int32 i = 0; i < sMultiPolyline.Parts.Count; i++)
                {
                    MyMapObjects.moPoints sPoints = sMultiPolyline.Parts.GetItem(i);
                    for (Int32 j = 0; j < sPoints.Count - 1; j++)
                    {
                        MyMapObjects.moPoints tempPoints = new MyMapObjects.moPoints();
                        tempPoints.Add(sPoints.GetItem(j));
                        tempPoints.Add(sPoints.GetItem(j + 1));
                        tempPoints.UpdateExtent();
                        if (MyMapObjects.moMapTools.IsPointOnPolyline(sPoint, tempPoints, sTolerance))
                        {
                            mMouseOnPartIndex = i;
                            mMouseOnPointIndex = j;
                            return true;
                        }
                    }
                }
            }
            return false;
        }

        //判断点是否靠近多点
        private bool PointCloseToPoints(MyMapObjects.moPoint sPoint, MyMapObjects.moPoints sPoints, double sTolerance)
        {
            if (mIsInMovePoint || mIsInDeletePoint)
            {
                for (Int32 i = 0; i < sPoints.Count; i++)
                {
                    if (MyMapObjects.moMapTools.IsPointOnPoint(sPoint, sPoints.GetItem(i), sTolerance))
                    {
                        mMouseOnPartIndex = 0;
                        mMouseOnPointIndex = i;
                        return true;
                    }
                }
            }
            if (mIsInAddPoint)
            {
                mMouseOnPartIndex = 0;
                mMouseOnPointIndex = sPoints.Count;
                return true;
            }
            return false;
        }

        //删除编辑记录
        private void DeleteEditPointRecord()
        {
            mEditPointOperation.Clear();
            mEditPointRecord.Clear();
            mPastPartIndex.Clear();
            mPastPointIndex.Clear();
        }

        //获取当前操作图层索引
        private Int32 GetOpLayerIndex()
        {
            if (SelectLayer.SelectedIndex == -1)
            {
                if (mEditMoMap == true) return -1;
                else return mLastOpLayerIndex;
            }
            else
            {
                return mLayerIndex[SelectLayer.SelectedIndex];
            }
        }

        //添加新图层后触发事件
        private void ThingsAfterNewLayer(MyMapObjects.moMapLayer sMapLayer, DataIOTools.gvShpFileManager sGvShpFileManager, DataIOTools.dbfFileManager sDbfFileManager)
        {
            //相关数据更新
            Int32 index = SearchInsertIndex(sMapLayer.ShapeType);
            mLastOpLayerIndex = index;
            moMap.Layers.Insert(index, sMapLayer);
            mGvShapeFiles.Insert(index, sGvShpFileManager);   //添加至要素文件管理列表
            mDbfFiles.Insert(index, sDbfFileManager); //添加至属性文件管理列表
            RefreshLayersTree();    //刷新图层列表
            if (moMap.Layers.Count == 1)
            {
                moMap.FullExtent();
            }
            else
            {
                moMap.RedrawMap();
            }
        }

        //复制
        private void CopySelectedFeatures()
        {
            MyMapObjects.moMapLayer sLayer = moMap.Layers.GetItem(mOperatingLayerIndex);
            mCopyingGeometries.Clear();
            if (sLayer.ShapeType == MyMapObjects.moGeometryTypeConstant.MultiPolygon)
            {
                mCopyingType = MyMapObjects.moGeometryTypeConstant.MultiPolygon;
                for (Int32 i = 0; i < sLayer.SelectedFeatures.Count; i++)
                {
                    MyMapObjects.moMultiPolygon sMultiPolygon = (MyMapObjects.moMultiPolygon)sLayer.SelectedFeatures.GetItem(i).Geometry;
                    mCopyingGeometries.Add(sMultiPolygon.Clone());
                }
            }
            else if (sLayer.ShapeType == MyMapObjects.moGeometryTypeConstant.MultiPolyline)
            {
                mCopyingType = MyMapObjects.moGeometryTypeConstant.MultiPolyline;
                for (Int32 i = 0; i < sLayer.SelectedFeatures.Count; i++)
                {
                    MyMapObjects.moMultiPolyline sMultiPolyline = (MyMapObjects.moMultiPolyline)sLayer.SelectedFeatures.GetItem(i).Geometry;
                    mCopyingGeometries.Add(sMultiPolyline.Clone());
                }
            }
            else if (sLayer.ShapeType == MyMapObjects.moGeometryTypeConstant.Point)
            {
                mCopyingType = MyMapObjects.moGeometryTypeConstant.Point;
                for (Int32 i = 0; i < sLayer.SelectedFeatures.Count; i++)
                {
                    MyMapObjects.moPoint sPoint = (MyMapObjects.moPoint)sLayer.SelectedFeatures.GetItem(i).Geometry;
                    mCopyingGeometries.Add(sPoint.Clone());
                }
            }
            else if (sLayer.ShapeType == MyMapObjects.moGeometryTypeConstant.MultiPoint)
            {
                mCopyingType = MyMapObjects.moGeometryTypeConstant.MultiPoint;
                for (Int32 i = 0; i < sLayer.SelectedFeatures.Count; i++)
                {
                    MyMapObjects.moPoints sPoints = (MyMapObjects.moPoints)sLayer.SelectedFeatures.GetItem(i).Geometry;
                    mCopyingGeometries.Add(sPoints.Clone());
                }
            }
        }

        //粘贴
        private void PasteSelectedFeatures()
        {
            mNeedToSave = true;
            MyMapObjects.moMapLayer sLayer = moMap.Layers.GetItem(mOperatingLayerIndex);
            if (mCopyingType == sLayer.ShapeType)
            {
                mNeedToSave = true;
                sLayer.SelectedFeatures.Clear();
                for (Int32 i = 0; i < mCopyingGeometries.Count; i++)
                {
                    MyMapObjects.moFeature sFeature = sLayer.GetNewFeature();
                    sFeature.Geometry = mCopyingGeometries[i];
                    sLayer.Features.Add(sFeature);
                }
                mCopyingGeometries.Clear();
                sLayer.UpdateExtent();
            }
            else
            {
                MessageBox.Show("图层类型与复制图形的几何类型不一致，无法粘贴！");
                return;
            }
        }

        //粘贴到新建图层
        public void PasteToNew()
        {
            MyMapObjects.moMapLayer sLayer = moMap.Layers.GetItem(mLastOpLayerIndex);
            sLayer.SelectedFeatures.Clear();
            for (Int32 i = 0; i < mCopyingGeometries.Count; i++)
            {
                MyMapObjects.moFeature sFeature = sLayer.GetNewFeature();
                sFeature.Geometry = mCopyingGeometries[i];
                sLayer.Features.Add(sFeature);
            }
            mCopyingGeometries.Clear();
            sLayer.UpdateExtent();
            SaveMapLayer(mLastOpLayerIndex);
        }

        private void SaveMapLayer(Int32 index)
        {
            //图形数据
            MyMapObjects.moMapLayer sLayer = moMap.Layers.GetItem(index);
            mGvShapeFiles[index].Geometries.Clear();
            for (Int32 j = 0; j < sLayer.Features.Count; j++)
            {
                mGvShapeFiles[index].Geometries.Add(sLayer.Features.GetItem(j).Geometry);
            }
            mGvShapeFiles[index].UpdateGeometries(mGvShapeFiles[index].Geometries);
            string path = mGvShapeFiles[index].DefaultFilePath;
            mGvShapeFiles[index].SaveToFile(path);
            //属性数据
            mDbfFiles[index].Fields = sLayer.AttributeFields;
            mDbfFiles[index].AttributesList.Clear();
            for (Int32 j = 0; j < sLayer.Features.Count; j++)
            {
                mDbfFiles[index].AttributesList.Add(sLayer.Features.GetItem(j).Attributes);
            }
            mDbfFiles[index].UpdateAttributesList(mDbfFiles[index].AttributesList);
            path = mDbfFiles[index].DefaultPath;
            mDbfFiles[index].SaveToFile(path);
        }
        private void 另存为ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            btnSaveProject_Click(sender, e);
        }

        private void 退出ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void 显示地理坐标ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mShowLngLat == false)
            {
                mShowLngLat = true;
                string text = tssCoordinate.Text;
                if (text != "")
                {
                    string[] temp = text.Split(',');
                    string posx = temp[0].Split(':')[1];
                    string posy = temp[1].Split(':')[1];
                    MyMapObjects.moPoint mousePoint = moMap.FromMapPoint(Convert.ToDouble(posx), Convert.ToDouble(posy));
                    ShowCoordinate(new PointF((float)mousePoint.X, (float)mousePoint.Y));
                }
                显示地理坐标ToolStripMenuItem.Text = "显示投影坐标";
            }
            else
            {
                mShowLngLat = false;
                string text = tssCoordinate.Text;
                if (text != "")
                {
                    string[] temp = text.Split(',');
                    string posx = temp[0].Split('°')[0];
                    string posy = temp[1].Split('°')[0];
                    MyMapObjects.moPoint sPoint = new MyMapObjects.moPoint(Convert.ToDouble(posx), Convert.ToDouble(posy));
                    MyMapObjects.moPoint mousePoint = moMap.ProjectionCS.TransferToProjCo(sPoint);
                    double sX = Math.Round(mousePoint.X);
                    double sY = Math.Round(mousePoint.Y);
                    tssCoordinate.Text = "X:" + sX.ToString() + ",Y:" + sY.ToString();
                }
                显示地理坐标ToolStripMenuItem.Text = "显示地理坐标";
            }
        }

        private void shpTogvshpToolStripMenuItem_Click(object sender, EventArgs e)
        {
            ShpToGvshp sTransform = new ShpToGvshp();
            sTransform.Owner = this;
            sTransform.ShowDialog();
        }

        private void 缩放至所选要素ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mLastOpLayerIndex == -1)
            {
                return;
            }
            MyMapObjects.moMapLayer sLayer = moMap.Layers.GetItem(mLastOpLayerIndex);
            Int32 sCount = sLayer.SelectedFeatures.Count;
            if (sCount == 0)
            {
                return;
            }
            double sMinX = double.MaxValue;
            double sMaxX = double.MinValue;
            double sMinY = double.MaxValue;
            double sMaxY = double.MinValue;
            for (Int32 i = 0; i < sCount; i++)
            {
                MyMapObjects.moRectangle sRect = sLayer.SelectedFeatures.GetItem(i).GetEnvelope();
                sMinX = Math.Min(sMinX, sRect.MinX);
                sMinY = Math.Min(sMinY, sRect.MinY);
                sMaxX = Math.Max(sMaxX, sRect.MaxX);
                sMaxY = Math.Max(sMaxY, sRect.MaxY);
            }
            MyMapObjects.moRectangle nRect = new MyMapObjects.moRectangle(sMinX, sMaxX, sMinY, sMaxY);
            moMap.SetExtent(nRect);
        }

        private void 平移至所选要素ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (mLastOpLayerIndex == -1)
            {
                return;
            }
            MyMapObjects.moMapLayer sLayer = moMap.Layers.GetItem(mLastOpLayerIndex);
            Int32 sCount = sLayer.SelectedFeatures.Count;
            if (sCount == 0)
            {
                return;
            }
            double sMinX = double.MaxValue;
            double sMaxX = double.MinValue;
            double sMinY = double.MaxValue;
            double sMaxY = double.MinValue;
            for (Int32 i = 0; i < sCount; i++)
            {
                MyMapObjects.moRectangle sRect = sLayer.SelectedFeatures.GetItem(i).GetEnvelope();
                sMinX = Math.Min(sMinX, sRect.MinX);
                sMinY = Math.Min(sMinY, sRect.MinY);
                sMaxX = Math.Max(sMaxX, sRect.MaxX);
                sMaxY = Math.Max(sMaxY, sRect.MaxY);
            }
            double sX = (sMinX + sMaxX) / 2;
            double sY = (sMinY + sMaxY) / 2;
            MyMapObjects.moRectangle sExtent = moMap.GetExtent();
            double mX = (sExtent.MinX + sExtent.MaxX) / 2;
            double mY = (sExtent.MinY + sExtent.MaxY) / 2;

            moMap.PanDelta(mX - sX, mY - sY);
        }

        private void 清除所选要素ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            for (Int32 i = 0; i < moMap.Layers.Count; i++)
            {
                MyMapObjects.moMapLayer sLayer = moMap.Layers.GetItem(i);
                sLayer.SelectedFeatures.Clear();
            }
            moMap.RedrawMap();
        }

        private void btnOpenProject_Click(object sender, EventArgs e)
        {
            打开地图ToolStripMenuItem_Click(sender, e);
        }

        private void geoView帮助ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("想想ArcGIS,QGIS怎么做");
        }

        private void 关于GeoViewToolStripMenuItem_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Geo-GroupOne 由曹仁君，张哲玮，唐金潼，陈梦玺，徐嘉辰联合开发");
        }

        #endregion

        #region 数据库连接

        private void 连接数据库ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            postsqlinfo infowindows= new postsqlinfo(this);//导入本窗口就可以了
            infowindows.Owner = this;
            infowindows.Show();//展示
            //moMap.mPostgisConnect = new MyMapObjects.PostGISConnect();//实现这个类
            //string order = "SELECT ST_AsText(geom) AS geom FROM public.province";
            //DataTable dt=moMap.mPostgisConnect.Query(order);
        }
        private void 从数据库打开ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //点击这个之后，会弹出一个对话框，输入sql命令
            Postsqlorder orderwindows = new Postsqlorder(this);//导入本窗口就可以了
            orderwindows.Owner = this;
            orderwindows.Show();//展示
        }
        public void Refresh_picture(MyMapObjects.moMapLayer sMapLayer)
        {
            Int32 index = SearchInsertIndex(sMapLayer.ShapeType);
            mLastOpLayerIndex = index;
            moMap.Layers.Insert(index, sMapLayer);
            RefreshLayersTree();    //刷新图层列表
            if (moMap.Layers.Count == 1)
            {
                moMap.FullExtent();
            }
            else
            {
                moMap.RedrawMap();
            }
        }
        private void 保存至数据库ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Postgischangeorder orderwindows = new Postgischangeorder(this);//导入本窗口就可以了
            orderwindows.Owner = this;
            orderwindows.Show();//展示
        }



        #endregion


    }
}
