﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace GeoView
{
    public partial class PointRenderer : Form
    {
        #region 字段

        private Int32 mRendererMode = 0; //渲染方式,0:简单渲染,1:唯一值渲染,2:分级渲染
        private Int32 mSymbolStyle = 0; //样式索引
        //简单渲染参数
        private Color mSimpleRendererColor = Color.Red; //符号颜色
        private Double mSimpleRendererSize = 5; //符号尺寸
        //唯一值渲染参数
        private Int32 mUniqueFieldIndex = 0; //绑定字段索引
        private Double mUniqueRendererSize = 5; //符号尺寸
        //分级渲染参数
        private Int32 mClassBreaksFieldIndex = 0; //绑定字段索引
        private Int32 mClassBreaksNum = 5; //分类数
        private Color mClassBreaksRendererColor = Color.Red; //符号颜色
        private Double mClassBreaksRendererMinSize = 3; //符号起始尺寸,点图层采用符号尺寸进行分级表示
        private Double mClassBreaksRendererMaxSize = 6; //符号终止尺寸

        #endregion

        #region 构造函数
        public PointRenderer(MyMapObjects.moMapLayer layer)
        {
            InitializeComponent();
            //填充样式下拉列表
            cboStyle.Items.Add("Circle");
            cboStyle.Items.Add("SolidCircle");
            cboStyle.Items.Add("Triangle");
            cboStyle.Items.Add("SolidTriangle");
            cboStyle.Items.Add("Square");
            cboStyle.Items.Add("SolidSquare");
            cboStyle.Items.Add("CircleDot");
            cboStyle.Items.Add("CircleCircle");
            //填充字段下拉列表
            Int32 fieldCount = layer.AttributeFields.Count;
            for (Int32 i = 0; i < fieldCount; i++)
            {
                cboUniqueField.Items.Add(layer.AttributeFields.GetItem(i).Name);
                cboClassBreaksField.Items.Add(layer.AttributeFields.GetItem(i).Name);
            }
            cboStyle.SelectedIndex = 0;
            cboUniqueField.SelectedIndex = 0;
            cboClassBreaksField.SelectedIndex = 0;


            if (layer.Renderer.RendererType == MyMapObjects.moRendererTypeConstant.Simple)
            {
                MyMapObjects.moSimpleRenderer sRenderer = (MyMapObjects.moSimpleRenderer)layer.Renderer;
                MyMapObjects.moSimpleMarkerSymbol sSymbol = (MyMapObjects.moSimpleMarkerSymbol)sRenderer.Symbol;
                cboStyle.SelectedIndex = (Int32)sSymbol.Style;
                btnSimpleColor.BackColor = sSymbol.Color;
                mSimpleRendererColor = sSymbol.Color;
                nudSimpleSize.Value = Convert.ToDecimal(sSymbol.Size);
                rbtnSimple.Checked = true;

            }
            else if (layer.Renderer.RendererType == MyMapObjects.moRendererTypeConstant.UniqueValue)
            {
                MyMapObjects.moUniqueValueRenderer sRenderer = (MyMapObjects.moUniqueValueRenderer)layer.Renderer;
                MyMapObjects.moSimpleMarkerSymbol sSymbol = (MyMapObjects.moSimpleMarkerSymbol)sRenderer.GetSymbol(0);
                cboStyle.SelectedIndex = (Int32)sSymbol.Style;
                cboUniqueField.SelectedIndex = layer.AttributeFields.FindField(sRenderer.Field);
                nudUniqueSize.Value = Convert.ToDecimal(sSymbol.Size);
                rbtnUniqueValue.Checked = true;

            }
            else if (layer.Renderer.RendererType == MyMapObjects.moRendererTypeConstant.ClassBreaks)
            {
                MyMapObjects.moClassBreaksRenderer sRenderer = (MyMapObjects.moClassBreaksRenderer)layer.Renderer;
            
                MyMapObjects.moSimpleMarkerSymbol sStartSymbol = (MyMapObjects.moSimpleMarkerSymbol)sRenderer.GetSymbol(0);
                MyMapObjects.moSimpleMarkerSymbol sEndSymbol = (MyMapObjects.moSimpleMarkerSymbol)sRenderer.GetSymbol(sRenderer.BreakCount-1);
                cboStyle.SelectedIndex = (Int32)sStartSymbol.Style;
                btnClassBreaksColor.BackColor = sStartSymbol.Color;
                mClassBreaksRendererColor = sStartSymbol.Color;
                cboClassBreaksField.SelectedIndex = layer.AttributeFields.FindField(sRenderer.Field);
                nudClassBreaksNum.Value = sRenderer.BreakCount;
                nudClassBreaksMinSize.Value = Convert.ToDecimal(sStartSymbol.Size);
                nudClassBreaksMaxSize.Value = Convert.ToDecimal(sEndSymbol.Size);
                rbtnClassBreaks.Checked = true;
            }
            SetEnabled();
        }

        #endregion

        #region 窗体操作

        //设置选项是否可选
        private void SetEnabled()
        {
            if (rbtnSimple.Checked)
            {
                btnSimpleColor.Enabled = true;
                nudSimpleSize.Enabled = true;
                cboUniqueField.Enabled = false;
                nudUniqueSize.Enabled = false;
                cboClassBreaksField.Enabled = false;
                nudClassBreaksNum.Enabled = false;
                btnClassBreaksColor.Enabled = false;
                nudClassBreaksMinSize.Enabled = false;
                nudClassBreaksMaxSize.Enabled = false;
            }
            else if (rbtnUniqueValue.Checked)
            {
                btnSimpleColor.Enabled = false;
                nudSimpleSize.Enabled = false;
                cboUniqueField.Enabled = true;
                nudUniqueSize.Enabled = true;
                cboClassBreaksField.Enabled = false;
                nudClassBreaksNum.Enabled = false;
                btnClassBreaksColor.Enabled = false;
                nudClassBreaksMinSize.Enabled = false;
                nudClassBreaksMaxSize.Enabled = false;
            }
            else if (rbtnClassBreaks.Checked)
            {
                btnSimpleColor.Enabled = false;
                nudSimpleSize.Enabled = false;
                cboUniqueField.Enabled = false;
                nudUniqueSize.Enabled = false;
                cboClassBreaksField.Enabled = true;
                nudClassBreaksNum.Enabled = true;
                btnClassBreaksColor.Enabled = true;
                nudClassBreaksMinSize.Enabled = true;
                nudClassBreaksMaxSize.Enabled = true;
            }
        }

        private void rbtnSimple_CheckedChanged(object sender, EventArgs e)
        {
            SetEnabled();
        }

        private void rbtnUniqueValue_CheckedChanged(object sender, EventArgs e)
        {
            SetEnabled();
        }

        private void rbtnClassBreaks_CheckedChanged(object sender, EventArgs e)
        {
            SetEnabled();
        }
        //选择渲染方式
        private void GetRendererMode()
        {
            if (rbtnSimple.Checked)
            {
                mRendererMode = 0;              
            }
            else if (rbtnUniqueValue.Checked)
            {
                mRendererMode = 1;
            }
            else if (rbtnClassBreaks.Checked)
            {
                mRendererMode = 2;
            }
        }
        //选择样式
        private void cboStyle_SelectedIndexChanged(object sender, EventArgs e)
        {
            mSymbolStyle = cboStyle.SelectedIndex;
        }

        //选择简单渲染符号颜色
        private void btnSimpleColor_Click(object sender, EventArgs e)
        {
            DialogResult sColorDialogResult = cldPointRenderer.ShowDialog();
            if (sColorDialogResult == DialogResult.OK)
            {
                mSimpleRendererColor = cldPointRenderer.Color;
                btnSimpleColor.BackColor = mSimpleRendererColor;
            }
        }

        //选择简单渲染符号大小
        private void nudSimpleSize_ValueChanged(object sender, EventArgs e)
        {
            mSimpleRendererSize = (Double)nudSimpleSize.Value;
        }

        //选择唯一值渲染字段
        private void cboUniqueField_SelectedIndexChanged(object sender, EventArgs e)
        {
            mUniqueFieldIndex = cboUniqueField.SelectedIndex;
        }

        //选择唯一值渲染符号大小
        private void nudUniqueSize_ValueChanged(object sender, EventArgs e)
        {
            mUniqueRendererSize = (Double)nudUniqueSize.Value;
        }

        //选择分级渲染字段
        private void cboClassBreaksField_SelectedIndexChanged(object sender, EventArgs e)
        {
            mClassBreaksFieldIndex = cboClassBreaksField.SelectedIndex;
        }

        //选择分级渲染分级数
        private void nudClassBreaksNum_ValueChanged(object sender, EventArgs e)
        {
            mClassBreaksNum = (Int32)nudClassBreaksNum.Value;
        }

        //选择分级渲染符号颜色
        private void btnClassBreaksColor_Click(object sender, EventArgs e)
        {
            DialogResult sColorDialogResult = cldPointRenderer.ShowDialog();
            if (sColorDialogResult == DialogResult.OK)
            {
                mClassBreaksRendererColor = cldPointRenderer.Color;
                btnClassBreaksColor.BackColor = mClassBreaksRendererColor;
            }
        }

        //选择分级渲染符号最小尺寸
        private void nudClassBreaksMinSize_ValueChanged(object sender, EventArgs e)
        {
            mClassBreaksRendererMinSize = (Double)nudClassBreaksMinSize.Value;
        }

        //选择分级渲染符号最大尺寸
        private void nudClassBreaksMaxSize_ValueChanged(object sender, EventArgs e)
        {
            Double tmpSize = (Double)nudClassBreaksMaxSize.Value;
            if (tmpSize < mClassBreaksRendererMinSize)
            {
                MessageBox.Show("终止尺寸必须大于起始尺寸,请重新设置!");
                return;
            }
            else
            {
                mClassBreaksRendererMaxSize = tmpSize;
            }
        }

        //确认
        private void btnPointRendererConfirm_Click(object sender, EventArgs e)
        {
            Main main = (Main)this.Owner;
            GetRendererMode();
            main.GetPointRenderer(mRendererMode, mSymbolStyle, mSimpleRendererColor, mSimpleRendererSize,
                mUniqueFieldIndex, mUniqueRendererSize, mClassBreaksFieldIndex, mClassBreaksNum,
                mClassBreaksRendererColor, mClassBreaksRendererMinSize, mClassBreaksRendererMaxSize);
            this.Close();
        }

        //取消
        private void btnPointRendererCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }
        #endregion

        
    }
}
