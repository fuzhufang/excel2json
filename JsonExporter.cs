﻿using System;
using System.IO;
using System.Data;
using System.Text;
using System.Collections.Generic;
using Newtonsoft.Json;

namespace excel2json
{
    /// <summary>
    /// 将DataTable对象，转换成JSON string，并保存到文件中
    /// </summary>
    class JsonExporter
    {
        List<Dictionary<string, object>> m_data;

        /// <summary>
        /// 构造函数：完成内部数据创建
        /// </summary>
        /// <param name="sheet">ExcelReader创建的一个表单</param>
        /// <param name="headerRows">表单中的那几行是表头</param>
        public JsonExporter(DataTable sheet, int headerRows, bool lowcase)
        {
            if (sheet.Columns.Count <= 0)
                return;
            if (sheet.Rows.Count <= 0)
                return;

            m_data = new List<Dictionary<string, object>>();

            DataRow typeRow = sheet.Rows[0];
            DataRow commentRow = sheet.Rows[1];

            //--以第一列为ID，转换成ID->Object的字典
            int firstDataRow = headerRows - 1;
            for (int i = firstDataRow; i < sheet.Rows.Count; i++)
            {
                DataRow row = sheet.Rows[i];
                string ID = row[sheet.Columns[0]].ToString();
                if (ID.Length <= 0)
                    continue;

                var rowData = new Dictionary<string, object>();
                foreach (DataColumn column in sheet.Columns)
                {
                    string filedName = column.ToString();
                    string filedType = typeRow[column].ToString();
                    string fieldComment = commentRow[column].ToString();

                    object value = row[column];
                    // 去掉数值字段的“.0”
                    if (value.GetType() == typeof(double))
                    {
                        double num = (double)value;
                        if ((int)num == num)
                            value = (int)num;
                    }
                    string fieldName = column.ToString();
                    // 表头自动转换成小写
                    if (lowcase)    
                        fieldName = fieldName.ToLower();

                    var s1 = value.ToString();
                    var s2 = value.GetType();

                    // excel里定义字符串类型，对象为空默认长度为0字符串
                    if (s2.FullName== "System.DBNull" && ((filedType.IndexOf("varchar", 0) > -1) || (filedType.IndexOf("char", 0) > -1)))
                    {
                        value = "";
                    }

                    // excel里定义整数类型，对象为空默认0
                    if (s2.FullName == "System.DBNull" && ((filedType.IndexOf("int", 0) > -1) || (filedType.IndexOf("unsigned", 0) > -1)))
                    {
                        value = 0;
                    }

                    // excel里定义浮点数类型，对象为空默认0
                    if (s2.FullName == "System.DBNull" && ((filedType.IndexOf("float", 0) > -1) || (filedType.IndexOf("double", 0) > -1)))
                    {
                        value = 0;
                    }

                    if (!string.IsNullOrEmpty(fieldName))
                    {
                        rowData[fieldName] = value;
                    }
                    else
                    {
                    }
                }

                m_data.Add(rowData);
            }
        }

        /// <summary>
        /// 将内部数据转换成Json文本，并保存至文件
        /// </summary>
        /// <param name="jsonPath">输出文件路径</param>
        public void SaveToFile(string filePath, Encoding encoding)
        {
            if (m_data == null)
                throw new Exception("JsonExporter内部数据为空。");

            //-- 转换为JSON字符串
            string json = JsonConvert.SerializeObject(m_data, Formatting.Indented);

            //-- 保存文件
            using (FileStream file = new FileStream(filePath, FileMode.Create, FileAccess.Write))
            {
                using (TextWriter writer = new StreamWriter(file, encoding))
                    writer.Write(json);
            }
        }
    }
}
