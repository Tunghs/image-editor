using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.WindowsAPICodePack.Dialogs;
using System.IO;
using System.Diagnostics;
using OpenCvSharp;

namespace imageEditor
{
    public partial class Form1 : Form
    {
        // =================================== 전역 변수 =====================================
        // 경로
        private static string _Path = null;
        // 폴더인지 파일인지 구분해주는 변수
        private static string _Type = null;

        // =================================== 파일과 폴더 =====================================

        /// <summary>
        /// 경로에 파일이름이 포함 된 경우 파일 이름을 반환함
        /// </summary>
        /// <param name="path">경로</param>
        /// <returns></returns>
        private string GetFileName(string path)
        {
            // path를 \를 기준으로 잘라서 배열에 담는다.
            // split 함수를 사용하면 기본적으로 char 형으로 구분자를 사용해 문장을 자르기 때문에
            // 아래와 같이 사용한다.
            string[] dirArray = path.Split(new string[] { @"\" }, StringSplitOptions.None);

            // 배열의 길이를 구한 후 마지막 값이 들어있는 곳을 구한다.
            int num = dirArray.Length - 1;

            return dirArray[num];
        }

        /// <summary>
        /// 경로에 파일 이름이 포함 된 경우 파일 이름 빼고 경로만 반환함.
        /// </summary>
        /// <param name="path">경로</param>
        /// <returns></returns>
        private string GetFileDirUrl(string path)
        {
            string[] dirArray = path.Split(new string[] { @"\" }, StringSplitOptions.None);

            // 배열의 길이를 구한 후 마지막 값이 들어있는 곳을 구한다.
            int num = dirArray.Length - 1;

            string dirUrl = path.Replace(@"\" + dirArray[num], "");

            return dirUrl;
        }

        // =================================== 경로 불러오기 =====================================
 
        /// <summary>
        /// 폴더 or 파일을 선택하는 dialog
        /// check 값을 받아 폴더를 선택할지 파일을 선택할지 결정.
        /// </summary>
        /// <param name="check"></param>
        private void OpenDialog(bool check)
        {
            try 
            {
                textBox.Clear();

                CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                dialog.InitialDirectory = "C:\\";
                dialog.IsFolderPicker = check;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok) ;
                {
                    _Path = dialog.FileName;
                    textBox.Text = _Path;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("취소하셨습니다.");
            }
        }

        // =================================== 리스트 출력 =====================================
        // 선택한 폴더 내의 파일 리스트에 출력
        private void ShowFileListBox(string path)
        {
            beforeFileList.Items.Clear();
            afterFileList.Items.Clear();

            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(path);

            foreach (System.IO.FileInfo File in dir.GetFiles())
            {
                string fileName = File.Name.Substring(0, File.Name.Length);

                beforeFileList.Items.Add(fileName);
            }
        }

        // 에디터 기능 적용 후 변경된 파일을 리스트에 보여줌
        private void ShowResultFileList(string path)
        {
            afterFileList.Items.Clear();

            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(path);

            foreach (System.IO.FileInfo File in dir.GetFiles())
            {
                string fileName = File.Name.Substring(0, File.Name.Length);

                afterFileList.Items.Add(fileName);
            }
        }

        // =================================== 파일 이름 관련 기능 =====================================

        /// <summary>
        /// 파일 이름 관련 기능 선택
        /// </summary>
        /// <param name="before">이전 단어</param>
        /// <param name="after">변경 단어</param>
        /// <param name="add">추가 단어</param>
        private void SelectReFunction(string before, string after, string add)
        {
            // 단어를 찾아서 변경&추가하기
            if (before != "" && after != "" && add != "")
            {
                FileReName(before, after);
                AddName(add);
            }
            // 단어만 찾아서 변경(특정 단어를 지우려고 하는 경우)
            else if (before != "" && after == "" && add == "")
            {
                FileReName(before, after);
            }
            // 단어만 찾아서 변경
            else if (before != "" && after != "" && add == "")
            {
                FileReName(before, after);
            }
            // 단어를 추가
            else if (before == "" && after == "" && add != "")
            {
                AddName(add);
            }
            else
            {
                MessageBox.Show("입력되지 않은 항목이 있습니다.");
            }
        }

        /// <summary>
        /// 해당 경로에 파일이 존재하는지 확인.
        /// </summary>
        /// <param name="fileUrl">파일의 경로</param>
        private bool FileCheck(string fileUrl)
        {
            if (System.IO.File.Exists(fileUrl))
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// 파일 이름 변경
        /// </summary>
        /// <param name="beforeWord">이전 단어</param>
        /// <param name="afterWord">변경 단어</param>
        private void FileReName(string beforeWord, string afterWord)
        {
            // 일치하는 단어가 있는지 확인하는 변수
            string check = "false";

            // type 별로 처리를 다르게함.
            if (_Type == "file")
            {
                afterFileList.Items.Clear();

                if (GetFileName(_Path).Contains(beforeWord))
                {
                    string newfileName = GetFileName(_Path).Replace(beforeWord, afterWord);
                    string newFileUrl = GetFileDirUrl(_Path) + @"\" + newfileName;

                    System.IO.File.Move(_Path, newFileUrl);
                    check = "true";
                    _Path = newFileUrl;
                    afterFileList.Items.Add(GetFileName(_Path));
                }
            }
            else
            {
                System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(_Path);
                
                foreach (System.IO.FileInfo File in dir.GetFiles())
                {
                    string fileName = File.Name.Substring(0, File.Name.Length);
                    string fileUrl = _Path + @"\" + fileName;

                    if (FileCheck(fileUrl))
                    {
                        // 변경할 단어가 포함되어있다면 실행
                        if (fileName.Contains(beforeWord))
                        {
                            // 이름 변경
                            string newfileName = fileName.Replace(beforeWord, afterWord);
                            string newFileUrl = _Path + @"\" + newfileName;

                            System.IO.File.Move(fileUrl, newFileUrl);
                            check = "true";
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
                if (check == "true")
                {
                    ShowResultFileList(_Path);
                }
            }
            if (check == "false")
            {
                MessageBox.Show("일치하는 단어가 없습니다.");
            }
        }

        /// <summary>
        /// 파일 이름 추가
        /// </summary>
        /// <param name="addWord">추가할 단어</param>
        private void AddName(string addWord)
        {
            string option = addOptionComboBox.SelectedItem.ToString();

            if (_Type == "file")
            {
                afterFileList.Items.Clear();

                string newfileUrl = GetFileDirUrl(_Path) + @"\" + addWord + GetFileName(_Path);
                if (option == "이름 뒤")
                {
                    newfileUrl = GetFileDirUrl(_Path) + @"\" + GetFileName(_Path) + addWord;
                }

                System.IO.File.Move(_Path, newfileUrl);

                Debug.WriteLine(_Path);
                _Path = newfileUrl;
                Debug.WriteLine(_Path);
                afterFileList.Items.Add(GetFileName(_Path));
            }
            else
            {
                System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(_Path);

                foreach (System.IO.FileInfo File in dir.GetFiles())
                {
                    string fileName = File.Name.Substring(0, File.Name.Length);
                    string fileUrl = _Path + @"\" + fileName;
                    // default값은 이름 앞
                    string newfileUrl = _Path + @"\" + addWord + fileName;

                    if (option == "이름 뒤")
                    {
                        newfileUrl = _Path + @"\" + fileName + addWord;
                    }

                    System.IO.File.Move(fileUrl, newfileUrl);
                }
                ShowResultFileList(_Path);
            }
        }

        // =================================== 파일 섞기 =====================================
        private void AutoSet(string path)
        {

        }


        // =================================== 이미지 처리 =====================================

        /// <summary>
        /// 이미지 리사이즈
        /// </summary>
        /// <param name="path">경로</param>
        /// <param name="width">너비</param>
        /// <param name="height">높이</param>
        private void ReSize(string path, int width, int height)
        {
            // 사진파일이 아니면 Cv2.ImRead 에서 파일을 읽어오지 못하는 문제때문에 예외처리
            try
            {
                if (_Type == "file")
                {
                    Mat src = Cv2.ImRead(path, ImreadModes.Unchanged);
                    Mat dst = new Mat();

                    Cv2.Resize(src, dst, new OpenCvSharp.Size(width, height), 0, 0, InterpolationFlags.Cubic);
                    Cv2.ImWrite(path, dst);

                    MessageBox.Show("변경되었습니다.");
                }
                else
                {
                    System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(path);

                    foreach (System.IO.FileInfo File in dir.GetFiles())
                    {
                        string fileName = File.Name.Substring(0, File.Name.Length);
                        string fileUrl = path + @"\" + fileName;

                        Mat src = Cv2.ImRead(fileUrl, ImreadModes.Unchanged);
                        Mat dst = new Mat();

                        Cv2.Resize(src, dst, new OpenCvSharp.Size(width, height), 0, 0, InterpolationFlags.Cubic);
                        Cv2.ImWrite(fileUrl, dst);
                    }
                    MessageBox.Show("변경되었습니다.");
                }
            }
            catch
            {
                MessageBox.Show("사진 파일만 가능합니다.");
            }
        }

        /// <summary>
        /// 이미지 자르기
        /// </summary>
        /// <param name="path">경로</param>
        /// <param name="width">폭</param>
        /// <param name="height">높이</param>
        private void Crop(string path, int width, int height)
        {
            try
            {
                if (_Type == "file")
                {
                    // 매트릭스 형태로 파일을 불러온 후 크롭을 위해 비트맵 이미지로 변환
                    Mat src = Cv2.ImRead(path, ImreadModes.Color);
                    Bitmap bitSrc = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(src);

                    int x = 0 + (width / 2);
                    int y = 0 + (height / 2);
                    int newWidth = bitSrc.Width - width;
                    int newHeight = bitSrc.Height - height;

                    // (x 위치값, y의 위치값, 새로 적용할 width, 새로 적용할 height)
                    bitSrc = bitSrc.Clone(new Rectangle(x, y, newWidth, newHeight),
                             System.Drawing.Imaging.PixelFormat.DontCare);

                    Mat changeSrc = OpenCvSharp.Extensions.BitmapConverter.ToMat(bitSrc);

                    Cv2.ImWrite(path, changeSrc);
                    MessageBox.Show("변경되었습니다.");
                }
                else
                {
                    System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(path);

                    foreach (System.IO.FileInfo File in dir.GetFiles())
                    {
                        string fileName = File.Name.Substring(0, File.Name.Length);
                        string fileUrl = path + @"\" + fileName;

                        Mat src = Cv2.ImRead(fileUrl, ImreadModes.Color);
                        Bitmap bitSrc = OpenCvSharp.Extensions.BitmapConverter.ToBitmap(src);

                        int x = 0 + (width / 2);
                        int y = 0 + (height / 2);
                        int newWidth = bitSrc.Width - width;
                        int newHeight = bitSrc.Height - height;

                        // (x 위치값, y의 위치값, 새로 적용할 width, 새로 적용할 height)
                        bitSrc = bitSrc.Clone(new Rectangle(x, y, newWidth, newHeight),
                                 System.Drawing.Imaging.PixelFormat.DontCare);

                        Mat changeSrc = OpenCvSharp.Extensions.BitmapConverter.ToMat(bitSrc);

                        Cv2.ImWrite(fileUrl, changeSrc);
                    }
                    MessageBox.Show("변경되었습니다.");
                }
            }
            catch
            {
                MessageBox.Show("사진 파일만 가능합니다.");
            }
        }

        // =================================== Form =====================================
        public Form1()
        {
            InitializeComponent();
            // 콤보박스 기본값 설정
            addOptionComboBox.SelectedIndex = 0;
        }

        // 파일 경로
        private void openFileBtn_Click(object sender, EventArgs e)
        {
            try
            {
                _Type = "file";

                beforeFileList.Items.Clear();
                afterFileList.Items.Clear();

                OpenDialog(false);

                beforeFileList.Items.Add(GetFileName(_Path));
            }
            catch
            {
                _Path = null;
            }
        }

        // 폴더 경로
        private void openFolderBtn_Click(object sender, EventArgs e)
        {
            try
            {
                _Type = "folder";

                OpenDialog(true);
                ShowFileListBox(_Path);
            }
            catch
            {
                _Path = null;
            }
        }

        private void changeBtn_Click(object sender, EventArgs e)
        {
            string before = beforeWord.Text;
            string after = afterWord.Text;
            string add = addWord.Text;

            SelectReFunction(before, after, add);
        }

        private void resizeBtn_Click(object sender, EventArgs e)
        {
            // string을 int형으로 변환
            int width = int.Parse(resizeWidth.Text);
            int height = int.Parse(this.resizeHeight.Text);

            ReSize(_Path, width, height);
        }

        private void cropBtn_Click(object sender, EventArgs e)
        {
            string tWidth = cropWidth.Text;
            string tHeight = cropHeight.Text;

            if (tWidth == "") tWidth = "0";
            if (tHeight == "") tHeight = "0";

            int width = int.Parse(tWidth);
            int height = int.Parse(tHeight);

            Crop(_Path, width, height);
        }

        // 텍스트 박스에 숫자만 입력 가능하게 지정
        private void widthText_KeyPress(object sender, KeyPressEventArgs e)
        {
            OnlyNum(e);
        }

        private void heightText_KeyPress(object sender, KeyPressEventArgs e)
        {
            OnlyNum(e);
        }

        // 숫자만 입력 받게 해주는 함수.
        private void OnlyNum(KeyPressEventArgs e)
        {
            if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back)))
            {
                e.Handled = true;
            }
        }
    }
}
