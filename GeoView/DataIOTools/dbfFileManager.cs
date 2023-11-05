﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace GeoView.DataIOTools
{
    public class dbfFileManager
    {
        #region 字段

        private string _DefaultFilePath;   //文件路径
        private dbfFileHeader _dbfFileHeader;
        private MyMapObjects.moFields _Fields = new MyMapObjects.moFields();  //字段集合通过头文件读取，因此应当同时维护头文件和_Fields
        private List<MyMapObjects.moAttributes> _AttributesList = new List<MyMapObjects.moAttributes>();
        #endregion

        #region 构造函数

        /// <summary>
        /// 构造函数，用户读取文件
        /// </summary>
        /// <param name="filePath"></param>
        public dbfFileManager(string filePath)
        {
            _DefaultFilePath = filePath;
            FileStream sStream = new FileStream(filePath, FileMode.Open);
            BinaryReader sr = new BinaryReader(sStream);
            _dbfFileHeader = new dbfFileHeader(sr); //读取文件头
            CreateFieldsFromHeader();
            ReadAttributes(sr);
            sr.Close();
            sStream.Close();
        }

        /// <summary>
        /// 构造函数，用于创建文件
        /// </summary>
        public dbfFileManager()
        {
            _dbfFileHeader = new dbfFileHeader();
        }

        #endregion

        #region 属性

        /// <summary>
        /// 获取字段集合
        /// </summary>
        public MyMapObjects.moFields Fields
        {
            get { return _Fields; }
            set { _Fields = value; }
        }

        /// <summary>
        /// 获取属性列表
        /// </summary>
        public List<MyMapObjects.moAttributes> AttributesList
        {
            get { return _AttributesList; }
        }

        /// <summary>
        /// 默认路径
        /// </summary>
        public string DefaultPath
        {
            get { return _DefaultFilePath; }
            set { _DefaultFilePath = value; }
        }

        #endregion

        #region 方法
        /// <summary>
        /// 添加一个字段(默认追加在末尾)
        /// </summary>
        /// <param name="newField"></param>
        public void CreateField(MyMapObjects.moField newField, MyMapObjects.moAttributes newAttributes)
        {
            //（1）修改文件头
            MyMapObjects.moValueTypeConstant sValueType = newField.ValueType;
            dbfField sDbfField = new dbfField();
            string sFieldName = newField.Name;
            byte sDbfFieldType;
            byte sFieldLength;
            switch (sValueType)
            {
                case MyMapObjects.moValueTypeConstant.dInt32:
                    sDbfFieldType = (byte)'I';
                    sFieldLength = 11;
                    break;
                case MyMapObjects.moValueTypeConstant.dSingle:
                    sDbfFieldType = (byte)'F';
                    sFieldLength = 13;
                    break;
                case MyMapObjects.moValueTypeConstant.dDouble:
                    sDbfFieldType = (byte)'B';
                    sFieldLength = 22;
                    break;
                case MyMapObjects.moValueTypeConstant.dText:
                    sDbfFieldType = (byte)'C';
                    sFieldLength = 100;
                    break;
                default:
                    sDbfFieldType = (byte)'C';
                    sFieldLength = 100;
                    break;
            }
            _dbfFileHeader.HeaderLength += 32;
            _dbfFileHeader.RecordLength += sFieldLength;
            sDbfField.FieldName = sFieldName;
            sDbfField.FieldType = sDbfFieldType;
            sDbfField.FieldLength = sFieldLength;
            _dbfFileHeader.dbfFields.Add(sDbfField);
            //（2）修改字段集合
            _Fields.Append(newField);
            //（3）修改记录的属性值
            object[] sAttributesArray = newAttributes.ToArray();
            if (sAttributesArray.Length != _AttributesList.Count)
            {
                string error = "新增属性值数与记录数不一致!";
                throw new Exception(error);
            }
            /*
            for (Int32 i = 0; i < sAttributesArray.Length; ++i)
            {
                _AttributesList[i].Append(sAttributesArray[i]);
            }*/
        }

        /// <summary>
        /// 根据指定索引号删除字段
        /// </summary>
        public void DeleteField(Int32 index)
        {
            if (index < 0 || index >= _Fields.Count)
            {
                string error = "索引超出数组范围";
                throw new Exception(error);
            }
            //（1）修改文件头
            dbfField sDbfField = _dbfFileHeader.dbfFields[index];
            _dbfFileHeader.HeaderLength -= 32;
            _dbfFileHeader.RecordLength -= sDbfField.FieldLength;
            _dbfFileHeader.dbfFields.RemoveAt(index);
            //（2）修改字段集合
            _Fields.RemoveAt(index);
            /*
            //（3）修改记录的属性值
            for (Int32 i = 0; i < _AttributesList.Count; ++i)
            {
                _AttributesList[i].RemoveAt(index);
            }*/
        }

        /// <summary>
        /// 更新记录
        /// </summary>
        /// <param name="newAttributesList"></param>
        public void UpdateAttributesList(List<MyMapObjects.moAttributes> newAttributesList)
        {
            if (newAttributesList.Count != 0)
            {
                object[] sTempArray = newAttributesList[0].ToArray();
                if (sTempArray.Length != _Fields.Count)
                {
                    string error = "记录的属性值数与字段数不相等!";
                    throw new Exception(error);
                }
            }
            //（1）修改文件头
            _dbfFileHeader.RecordNum = (uint)newAttributesList.Count;
            //（2）修改记录的属性值
            _AttributesList = newAttributesList;
        }

        //保存至dbf文件，路径指定
        public void SaveToFile(string filePath)
        {
            FileStream sStream = new FileStream(filePath, FileMode.Create, FileAccess.Write);
            BinaryWriter sw = new BinaryWriter(sStream);
            RecordHeader(sw);
            RecordAttributes(sw);
            sw.Close();
            sStream.Close();
        }

        //保存至dbf文件，路径为原文件的路径(覆盖原文件内容)
        public void SaveToFile()
        {
            SaveToFile(_DefaultFilePath);
        }
        #endregion

        #region 私有函数

        //根据头文件读取字段
        private void CreateFieldsFromHeader()
        {
            for (Int32 i = 0; i < _dbfFileHeader.dbfFields.Count; ++i)
            {
                string sName = _dbfFileHeader.dbfFields[i].FieldName;
                char sdbfFieldType = (char)_dbfFileHeader.dbfFields[i].FieldType;
                MyMapObjects.moValueTypeConstant sValueType;
                switch (sdbfFieldType)
                {
                    case 'I':
                        sValueType = MyMapObjects.moValueTypeConstant.dInt32;
                        break;
                    case 'F':
                        sValueType = MyMapObjects.moValueTypeConstant.dSingle;
                        break;
                    case 'B':
                        sValueType = MyMapObjects.moValueTypeConstant.dDouble;
                        break;
                    case 'N':
                        sValueType = MyMapObjects.moValueTypeConstant.dDouble;
                        break;
                    default:
                        sValueType = MyMapObjects.moValueTypeConstant.dText;
                        break;
                }
                MyMapObjects.moField sField = new MyMapObjects.moField(sName, sValueType);
                _Fields.Append(sField);
            }
        }

        //读取要素的属性值
        private void ReadAttributes(BinaryReader sr)
        {
            sr.BaseStream.Seek(_dbfFileHeader.HeaderLength, SeekOrigin.Begin);
            UInt16 sRecordLength = _dbfFileHeader.RecordLength;
            for (Int32 i = 0; i < _dbfFileHeader.RecordNum; ++i)
            {
                byte[] sRecordContent = sr.ReadBytes(sRecordLength);
                MyMapObjects.moAttributes sAttributes = new MyMapObjects.moAttributes();
                Int32 sCurIndex = 1;
                for (Int32 j = 0; j < _dbfFileHeader.dbfFields.Count; ++j)
                {
                    dbfField sdbfField = _dbfFileHeader.dbfFields[j];
                    MyMapObjects.moValueTypeConstant sValueType = _Fields.GetItem(j).ValueType;
                    string sTemp = Encoding.UTF8.GetString(sRecordContent, sCurIndex, sdbfField.FieldLength).Trim((char)0x20).Replace("\0", "");
                    sCurIndex += sdbfField.FieldLength;
                    if (sValueType == MyMapObjects.moValueTypeConstant.dInt32)  //  由于dbf文件中不存在16位整数和64位整数类型，故不用管dInt16和dInt64
                    {
                        Int32 sTempValue = Convert.ToInt32(sTemp);
                        sAttributes.Append(sTempValue);
                    }
                    else if (sValueType == MyMapObjects.moValueTypeConstant.dSingle)
                    {
                        float sTempValue = Convert.ToSingle(sTemp);
                        sAttributes.Append(sTempValue);
                    }
                    else if (sValueType == MyMapObjects.moValueTypeConstant.dDouble)
                    {
                        double sTempValue = 0;
                        if (sTemp.Length != 0)
                        {
                            sTempValue = Convert.ToDouble(sTemp);
                        }
                        sAttributes.Append(sTempValue);
                    }
                    else
                    {
                        sAttributes.Append(sTemp);
                    }
                }
                _AttributesList.Add(sAttributes);
            }
        }

        //写文件头
        private void RecordHeader(BinaryWriter sw)
        {
            sw.Write(_dbfFileHeader.FileType);  //写入文件类型
            byte[] sLastModifyDate = new byte[3];   //接下来3个字节写入修改时的年月日
            sLastModifyDate[0] = Convert.ToByte(DateTime.Now.Year - 1900);  //年要减去1900
            sLastModifyDate[1] = Convert.ToByte(DateTime.Now.Month);
            sLastModifyDate[2] = (byte)(DateTime.Now.Day);
            sw.Write(sLastModifyDate);
            sw.Write(_dbfFileHeader.RecordNum);
            sw.Write(_dbfFileHeader.HeaderLength);
            sw.Write(_dbfFileHeader.RecordLength);
            //第12-31为保留字段，默认为0
            byte[] sTempBytes = new byte[20];
            sw.Write(sTempBytes);
            //接下来开始写入字段
            for (Int32 i = 0; i < _dbfFileHeader.dbfFields.Count; ++i)
            {
                dbfField sField = _dbfFileHeader.dbfFields[i];
                sw.Write(ConvertStringToBytes(sField.FieldName, 11));
                sw.Write(sField.FieldType);
                sw.Write(new byte[4]);  //12-15为保留字节，默认为0
                sw.Write(sField.FieldLength);
                sw.Write(new byte[15]);  //17-31均为保留字节，默认为0
            }
            //最后以0D结尾
            sw.Write((byte)0x0D);
        }

        //写记录
        private void RecordAttributes(BinaryWriter sw)
        {
            //（1）获取每一个字段的长度
            Int32[] sFieldLength = new Int32[_dbfFileHeader.dbfFields.Count];
            for (Int32 i = 0; i < _dbfFileHeader.dbfFields.Count; ++i)
            {
                sFieldLength[i] = _dbfFileHeader.dbfFields[i].FieldLength;
            }
            //（2）将记录逐行写入内存
            for (Int32 i = 0; i < _AttributesList.Count; ++i)
            {
                sw.Write((byte)0x20);   //每一行第一个字节默认为0x20
                object[] sAttributes = _AttributesList[i].ToArray();
                for (Int32 j = 0; j < sAttributes.Length; ++j)
                {
                    object sTempValue = sAttributes[j];
                    if (sTempValue != null)
                    {
                        sw.Write(ConvertStringToBytes(sTempValue.ToString(), sFieldLength[j]));
                    }
                    else
                    {
                        sw.Write(ConvertStringToBytes("", sFieldLength[j]));
                    }
                }
            }
        }

        private byte[] ConvertStringToBytes(string convertedStr, Int32 bytesLength)
        {
            byte[] sResultBytes = new byte[bytesLength];
            byte[] sTempBytes = Encoding.UTF8.GetBytes(convertedStr);
            if (sTempBytes.Length == bytesLength)
            {
                sResultBytes = sTempBytes;
            }
            else if (sTempBytes.Length > bytesLength)
            {
                Array.ConstrainedCopy(sTempBytes, 0, sResultBytes, 0, bytesLength);
            }
            else
            {
                Array.ConstrainedCopy(sTempBytes, 0, sResultBytes, 0, sTempBytes.Length);
            }
            return sResultBytes;
        }

        #endregion
    }
}