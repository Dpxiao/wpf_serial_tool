using System;
//using System.Threading.Tasks;
using System.Windows;

using NPOI.HSSF.UserModel;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using System.IO;
using System.Collections.Generic;

namespace WIoTa_Serial_Tool
{
    public partial class MainWindow : Window
    {
        // 将数据写入Excel文件

        public static IWorkbook CreateWorkbook(string filePath)
        {
            try
            {
                if (File.Exists(filePath))
                {
                    // 如果文件已经存在，则直接打开并返回工作簿对象
                    using (FileStream fs = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                    {
                        if (Path.GetExtension(filePath) == ".xls")
                        {
                            return new HSSFWorkbook(fs);
                        }
                        else if (Path.GetExtension(filePath) == ".xlsx")
                        {
                            return new XSSFWorkbook(fs);
                        }
                        else
                        {
                            throw new Exception("不支持的文件格式");
                        }
                    }
                }
                else
                {
                    // 如果文件不存在，则创建一个新的工作簿对象并返回
                    if (Path.GetExtension(filePath) == ".xls")
                    {
                        return new HSSFWorkbook();
                    }
                    else if (Path.GetExtension(filePath) == ".xlsx")
                    {
                        return new XSSFWorkbook();
                    }
                    else
                    {
                        throw new Exception("不支持的文件格式");
                    }
                }
            }
            catch (IOException ex)
            {
                MessageBox.Show($"{ex.Message}", "提示");
                throw new Exception("文件打开失败！");
            }
        }

        public static bool IsSheetNameExists(IWorkbook workbook, string sheetName)
        {
            for (int i = 0; i < workbook.NumberOfSheets; i++)
            {
                if (workbook.GetSheetName(i) == sheetName)
                {
                    return true;
                }
            }

            return false;
        }

        public static int IsSheetNameItemNum(IWorkbook workbook, string sheetName)
        {
            for (int i = 0; i < workbook.NumberOfSheets; i++)
            {
                if (workbook.GetSheetName(i) == sheetName)
                {
                    return i;
                }
            }

            return 0;
        }

        public static void WriteExcel(IWorkbook workbook, ISheet[] sheets, string filePath, string sheetName, int sheetNum, int tab_index, List<GridDataTemp> DataTemp)
        {
            workbook = CreateWorkbook(filePath);

            sheets[sheetNum] = !IsSheetNameExists(workbook, sheetName) ? workbook.CreateSheet(sheetName) : workbook.GetSheet(sheetName);

            for (int i = 0; i < DataTemp.Count; i++)
            {

                IRow row = sheets[sheetNum].CreateRow(i);
                switch (tab_index)
                {
                    case 0:
                        row.CreateCell(0).SetCellValue(DataTemp[i].Index);
                        row.CreateCell(1).SetCellValue(DataTemp[i].Hex);
                        row.CreateCell(2).SetCellValue(DataTemp[i].AT指令);
                        row.CreateCell(3).SetCellValue(DataTemp[i].发送);
                        break;
                    case 1:
                        
                        row.CreateCell(0).SetCellValue(DataTemp[i].Index);
                        row.CreateCell(1).SetCellValue(DataTemp[i].Hex);
                        row.CreateCell(2).SetCellValue(DataTemp[i].延时);
                        row.CreateCell(3).SetCellValue(DataTemp[i].AT指令);
                        row.CreateCell(4).SetCellValue(DataTemp[i].发送);
                        break;
                    case 2:
                        row.CreateCell(0).SetCellValue(DataTemp[i].Index);
                        row.CreateCell(1).SetCellValue(DataTemp[i].Hex);
                        row.CreateCell(2).SetCellValue(DataTemp[i].应答);
                        row.CreateCell(3).SetCellValue(DataTemp[i].延时);
                        row.CreateCell(4).SetCellValue(DataTemp[i].AT指令);
                        row.CreateCell(5).SetCellValue(DataTemp[i].发送);
                        break;
                    default:
                        row.CreateCell(0).SetCellValue(DataTemp[i].Index);
                        row.CreateCell(1).SetCellValue(DataTemp[i].Hex);
                        row.CreateCell(2).SetCellValue(DataTemp[i].AT指令);
                        row.CreateCell(3).SetCellValue(DataTemp[i].发送);
                        break;
                }


                // 创建一个单元格样式对象
                ICellStyle cellStyle = workbook.CreateCellStyle();
                cellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center; // 设置水平居中对齐

                //// 设置边框样式为实线
                //cellStyle.BorderTop = BorderStyle.Thin;
                //cellStyle.BorderBottom = BorderStyle.Thin;
                //cellStyle.BorderLeft = BorderStyle.Thin;
                //cellStyle.BorderRight = BorderStyle.Thin;

                // 将样式应用于单元格
                row.GetCell(0).CellStyle = cellStyle;
                row.GetCell(1).CellStyle = cellStyle;
                row.GetCell(2).CellStyle = cellStyle;
                row.GetCell(3).CellStyle = cellStyle;
            }

            try
            {
                using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
                {
                    workbook.Write(fileStream);
                }
                workbook.Close();
            }
            catch (IOException ex)
            {
                MessageBox.Show($"{ex.Message}", "提示");
                workbook.Close();
            }
        }

        //public static void WriteExcel(IWorkbook workbook, ISheet[] sheets, string filePath, string sheetName, int sheetNum, int tab_index, List<GridDataTemp> DataTemp)
        //{
        //    // Check if workbook already exists
        //    if (workbook == null)
        //        workbook = CreateWorkbook(filePath);

        //    // Get or create the sheet
        //    ISheet sheet;
        //    if (!IsSheetNameExists(workbook, sheetName))
        //    {
        //        sheet = workbook.CreateSheet(sheetName);
        //        sheets[sheetNum] = sheet;
        //    }
        //    else
        //    {
        //        sheet = workbook.GetSheet(sheetName);
        //    }

        //    // Clear existing rows in the sheet
        //    while (sheet.LastRowNum > 0)
        //    {
        //        sheet.RemoveRow(sheet.GetRow(sheet.LastRowNum));
        //    }

        //    for (int i = 0; i < DataTemp.Count; i++)
        //    {
        //        IRow row = sheet.CreateRow(i);
        //        switch (tab_index)
        //        {
        //            case 0:
        //                row.CreateCell(0).SetCellValue(DataTemp[i].Index);
        //                row.CreateCell(1).SetCellValue(DataTemp[i].Hex);
        //                row.CreateCell(2).SetCellValue(DataTemp[i].AT指令);
        //                row.CreateCell(3).SetCellValue(DataTemp[i].发送);
        //                break;
        //            case 1:
        //                row.CreateCell(0).SetCellValue(DataTemp[i].Index);
        //                row.CreateCell(1).SetCellValue(DataTemp[i].Hex);
        //                row.CreateCell(2).SetCellValue(DataTemp[i].延时);
        //                row.CreateCell(3).SetCellValue(DataTemp[i].AT指令);
        //                row.CreateCell(4).SetCellValue(DataTemp[i].发送);
        //                break;
        //            case 2:
        //                row.CreateCell(0).SetCellValue(DataTemp[i].Index);
        //                row.CreateCell(1).SetCellValue(DataTemp[i].Hex);
        //                row.CreateCell(2).SetCellValue(DataTemp[i].应答);
        //                row.CreateCell(3).SetCellValue(DataTemp[i].延时);
        //                row.CreateCell(4).SetCellValue(DataTemp[i].AT指令);
        //                row.CreateCell(5).SetCellValue(DataTemp[i].发送);
        //                break;
        //            default:
        //                row.CreateCell(0).SetCellValue(DataTemp[i].Index);
        //                row.CreateCell(1).SetCellValue(DataTemp[i].Hex);
        //                row.CreateCell(2).SetCellValue(DataTemp[i].AT指令);
        //                row.CreateCell(3).SetCellValue(DataTemp[i].发送);
        //                break;
        //        }

        //        // Create a cell style object
        //        ICellStyle cellStyle = workbook.CreateCellStyle();
        //        cellStyle.Alignment = NPOI.SS.UserModel.HorizontalAlignment.Center; // Set horizontal alignment to center

        //        // Apply the style to the cells
        //        row.GetCell(0).CellStyle = cellStyle;
        //        row.GetCell(1).CellStyle = cellStyle;
        //        row.GetCell(2).CellStyle = cellStyle;
        //        row.GetCell(3).CellStyle = cellStyle;
        //    }

        //    try
        //    {
        //        using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write))
        //        {
        //            workbook.Write(fileStream);
        //        }
        //    }
        //    catch (IOException ex)
        //    {
        //        MessageBox.Show($"{ex.Message}", "提示");
        //    }
        //    finally
        //    {
        //        // Close the workbook
        //        workbook.Close();
        //    }
        //}


        //读取EXCEL数据
        public static List<GridDataTemp> ReadExcel(string filePath,string sheetName,int tab_index)
        {
            List<GridDataTemp> DataTemp = new List<GridDataTemp>();
            IWorkbook workbook;
            try
            {
                using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    if (Path.GetExtension(filePath) == ".xls")
                        workbook = new HSSFWorkbook(fileStream);
                    else if (Path.GetExtension(filePath) == ".xlsx")
                        workbook = new XSSFWorkbook(fileStream);
                    else
                        throw new Exception("不支持的文件格式");

                    //判断文件是sheet是否存在
                    if (!IsSheetNameExists(workbook, sheetName))
                    {
                        ISheet sheets = workbook.CreateSheet(sheetName);

                        MessageBox.Show($"配置表格中不存在,{sheetName},创建新的表格!");
                        return DataTemp;//没有这个sheet就设置为空
                    }

                    ISheet sheet = workbook.GetSheetAt(IsSheetNameItemNum(workbook,sheetName));
                    if (sheet.LastRowNum <=0)
                    {
                        DataTemp.Clear();
                        for (int i = 1; i <= 60; i++)
                        {
                            DataTemp.Add(new GridDataTemp { Index = i, Hex = false, 应答 = "OK", 延时 = "1000", AT指令 = "AT", 发送 = $"发送按钮{i}" });
                        }
                    }
                    for (int i = 0; i <= sheet.LastRowNum; i++)
                    {
                        IRow row = sheet.GetRow(i);
                        if (row != null)
                        {
                            switch (tab_index)
                            {
                                case 0:
                                    DataTemp.Add(new GridDataTemp
                                    {
                                        Index = Convert.ToInt32(row.GetCell(0).ToString()),
                                        Hex = Convert.ToBoolean(row.GetCell(1).ToString()),
                                        AT指令 = row.GetCell(2).ToString(),
                                        发送 = row.GetCell(3).ToString()
                                    });
                                    break;
                                case 1:
                                    DataTemp.Add(new GridDataTemp
                                    {
                                        Index = Convert.ToInt32(row.GetCell(0).ToString()),
                                        Hex = Convert.ToBoolean(row.GetCell(1).ToString()),
                                        延时 = row.GetCell(2).ToString(),
                                        AT指令 = row.GetCell(3).ToString(),
                                        发送 = row.GetCell(4).ToString()
                                    });
                                    break;

                                case 2:
                                    DataTemp.Add(new GridDataTemp
                                    {
                                        Index = Convert.ToInt32(row.GetCell(0).ToString()),
                                        Hex = Convert.ToBoolean(row.GetCell(1).ToString()),
                                        应答 = row.GetCell(2).ToString(),
                                        延时 = row.GetCell(3).ToString(),
                                        AT指令 = row.GetCell(4).ToString(),
                                        发送 = row.GetCell(5).ToString()
                                    });

                                    break;
                                default:
                                    DataTemp.Add(new GridDataTemp
                                    {
                                        Index = Convert.ToInt32(row.GetCell(0).ToString()),
                                        Hex = Convert.ToBoolean(row.GetCell(1).ToString()),
                                        AT指令 = row.GetCell(2).ToString(),
                                        发送 = row.GetCell(3).ToString()
                                    });
                                    break;
                            }
                        }
                      
                    }
                  
                }
                workbook.Close();
                return DataTemp;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"{ex.Message}");
                return DataTemp;

            }
           
        }
    }

    // 读取Excel文件


   
}
