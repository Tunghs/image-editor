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
        // 파일 분류시 저장할 경로
        private static string _SaveFolderPath = null;

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

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok);
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

        private void OpenDialogSavePath(bool check)
        {
            try
            {
                SavePathTB.Clear();

                CommonOpenFileDialog dialog = new CommonOpenFileDialog();
                dialog.InitialDirectory = "C:\\";
                dialog.IsFolderPicker = check;

                if (dialog.ShowDialog() == CommonFileDialogResult.Ok) ;
                {
                    _SaveFolderPath = dialog.FileName;
                    SavePathTB.Text = _SaveFolderPath;
                }
            }
            catch (Exception e)
            {
                MessageBox.Show("취소하셨습니다.");
            }
        }

        // =================================== 하위폴더 및 파일 개수 =====================================

        /// <summary>
        /// 하위 디렉토리를 검색해서 리스트에 저장.
        /// </summary>
        /// <param name="path">지정한 경로</param>
        private List<string> serchSubDir(string path)
        {
            List<string> dirPathList = new List<string>();

            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(path);
            // 하위 디렉토리를 모두 검색하게 해주는 설정
            DirectoryInfo[] allDir = dir.GetDirectories("*", SearchOption.AllDirectories);

            if (dir.Exists)
            {
                foreach (System.IO.DirectoryInfo subDir in allDir)
                {
                    dirPathList.Add(subDir.FullName);
                }
                return dirPathList;
            }
            else
            {
                return dirPathList;
            }
        }

        /// <summary>
        /// 디렉토리 경로 포함 하위 디렉토리 파일의 개수를 보여줌.
        /// </summary>
        /// <param name="list"></param>
        private void AllDirFileNum(List<string> list)
        {
            FileNumLB.Text = "-";

            int fileNum = 0;

            foreach (string dirPath in list)
            {
                System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(dirPath);

                foreach (System.IO.FileInfo file in dir.GetFiles())
                {
                    fileNum++;
                }
            }
            FileNumLB.Text = fileNum.ToString();
        }

        /// <summary>
        /// SubDirPath에 하위 디렉토리의 절대경로를 보여줌
        /// </summary>
        /// <param name="list">하위 디렉토리 리스트</param>
        private void ShowSubDirList(List<string> list)
        {
            SubDirPath.Items.Clear();

            foreach (string subPath in list)
            {
                System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(subPath);
                // 절대 경로를 보여주기 위함.
                string AbsolutePath = dir.FullName.Replace(_Path, "");
                SubDirPath.Items.Add(AbsolutePath);
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

        // =================================== 파일 분류 =====================================

        /// <summary>
        /// 이미지를 분류해서 넣기 위한 Set디렉토리 아래에 train, validation, test 디렉토리를 생성함.
        /// </summary>
        /// <param name="path">저장할 경로</param>
        /// <returns></returns>
        private bool CreateSubDir(string path)
        {
            string trainDir = "TrainSet";
            string validationDir = "ValidationSet";
            string testDir = "TestSet";

            string setDir = "Set";

            string dirPath = path + @"\" + setDir;
            DirectoryInfo dir = new DirectoryInfo(dirPath);

            if (dir.Exists == false)
            {
                dir.Create();

                string setDirPath = path + @"\" + setDir;

                // 서브 디렉토리를 생성한다.
                string trainDirPath = setDirPath + @"\" + trainDir;
                string valDirPath = setDirPath + @"\" + validationDir;
                string testDirPath = setDirPath + @"\" + testDir;

                List<string> subDirList = new List<string>();

                subDirList.Add(trainDirPath);
                subDirList.Add(valDirPath);
                subDirList.Add(testDirPath);

                foreach (string subdirPath in subDirList)
                {
                    DirectoryInfo subDir = new DirectoryInfo(subdirPath);

                    if (subDir.Exists == false)
                    {
                        subDir.Create();
                    }
                }
                return true;
            }
            else
            {
                MessageBox.Show("Set 폴더가 이미 존재합니다!!!");
                return false;
            }
        }

        private void AutoSet(string loadPath, string savePath, int trainWeight, int valWeight, int testWeight)
        {
            // set 디렉토리가 존재하지 않다면 실행.
            if (CreateSubDir(savePath))
            {
                // 하위디렉토리 만큼 실행
                foreach (string subDir in serchSubDir(loadPath))
                {
                    // 가중치 구하는 부분
                    int fileNum = DirFileNum(subDir);
                    float weight = (float)fileNum / ((float)trainWeight + (float)valWeight + (float)testWeight);

                    float floatTrain = trainWeight * weight;
                    float floatVal = valWeight * weight;
                    float floatTest = testWeight * weight;

                    int intTrain = (int)floatTrain + (fileNum - ((int)floatTrain + (int)floatVal + (int)floatTest));
                    int intVal = (int)floatVal;
                    int intTest = (int)floatTest;

                    FileClassification(subDir, savePath, intTrain, intVal, intTest);
                }

                MessageBox.Show("생성");
            }
        }

        /// <summary>
        /// 디렉토리 내 파일 개수
        /// </summary>
        /// <param name="path">해당 디렉토리 경로</param>
        /// <returns></returns>
        private int DirFileNum(string path)
        {
            int fileNum = 0;

            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(path);
            foreach (System.IO.FileInfo file in dir.GetFiles())
            {
                fileNum++;
            }

            return fileNum;
        }

        private void FileClassification(string dirPath, string savePath, int trainWeight, int valWeight, int testWeight)
        {
            // 해당 디렉토리 내 파일들의 리스트 생성
            List<string> fileList = new List<string>();

            System.IO.DirectoryInfo dir = new System.IO.DirectoryInfo(dirPath);
            foreach (System.IO.FileInfo file in dir.GetFiles())
            {
                fileList.Add(file.FullName);
            }

            // train, validation, test 폴더에 넣을 파일들을 랜덤 추출해서 각자의 리스트에 저장
            List<string> trainList = new List<string>();
            List<string> valList = new List<string>();
            List<string> testList = new List<string>();

            trainList = RandomAddList(fileList, trainList, trainWeight);
            fileList = DelList(fileList, trainList);

            valList = RandomAddList(fileList, valList, valWeight);
            fileList = DelList(fileList, valList);

            testList = RandomAddList(fileList, testList, testWeight);
            fileList = DelList(fileList, testList);

            // 각 리스트에 저장된 파일들을 폴더로 이동
            string trainPath = savePath + @"\Set\TrainSet\";
            string valPath = savePath + @"\Set\ValidationSet\";
            string testPath = savePath + @"\Set\TestSet\";

            SaveFile(trainList, trainPath);
            SaveFile(valList, valPath);
            SaveFile(testList, testPath);
        }

        /// <summary>
        /// 리스트 랜덤 추출
        /// </summary>
        /// <param name="originalList">기존 리스트</param>
        /// <param name="setList">기존 리스트에서 랜덤으로 값을 뽑아 저장할 리스트</param>
        /// <param name="num">값의 개수</param>
        /// <returns></returns>
        private List<string> RandomAddList(List<string> originalList, List<string> setList, int num)
        {
            var random = new Random();

            for (int i = 0; i < num; i++)
            {
                int index = random.Next(originalList.Count);
                setList.Add(originalList[index]);
                originalList.Remove(originalList[index]);
            }
            return setList;
        }

        /// <summary>
        /// originalList값이 setList의 값에 들어있다면 제거
        /// </summary>
        /// <param name="originalList"></param>
        /// <param name="setList">제거할 값 리스트</param>
        /// <returns></returns>
        private List<string> DelList(List<string> originalList, List<string> setList)
        {
            foreach (string del in setList)
            {
                originalList.Remove(del);
            }
            return originalList;
        }

        /// <summary>
        /// 리스트에 저장된 파일을 특정 경로로 이동
        /// </summary>
        /// <param name="list"></param>
        /// <param name="savePath"></param>
        private void SaveFile(List<string> list, string savePath)
        {
            foreach (string path in list)
            {
                string[] fileNameArray = path.Split(new string[] { @"\" }, StringSplitOptions.None);
                int num = fileNameArray.Length - 1;
                string newSavePath = savePath + fileNameArray[num];
                Debug.WriteLine(path);
                Debug.WriteLine(savePath);
                System.IO.File.Move(path, newSavePath);
            }
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
            // 박스 기본값 설정
            addOptionComboBox.SelectedIndex = 0;
            resizeWidth.SelectedText = "0";
            resizeHeight.SelectedText = "0";
            cropWidth.SelectedText = "0";
            cropHeight.SelectedText = "0";

            TrainTB.SelectedText = "0";
            ValidationTB.SelectedText = "0";
            TestTB.SelectedText = "0";
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
                AllDirFileNum(serchSubDir(_Path));
                ShowSubDirList(serchSubDir(_Path));
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
            int height = int.Parse(resizeHeight.Text);

            // 모두 입력해야 작동할 수 있게 변경.
            if (width == 0 | height ==0)
            {
                MessageBox.Show("widht, height 값 모두 입력해주세요!");
            }
            else
            {
                ReSize(_Path, width, height);
            }
        }

        private void cropBtn_Click(object sender, EventArgs e)
        {
            int width = int.Parse(cropWidth.Text);
            int height = int.Parse(cropHeight.Text);

            Crop(_Path, width, height);
        }

        private void SavePathLoadBtn_Click(object sender, EventArgs e)
        {
            try
            {
                OpenDialogSavePath(true);
            }
            catch
            {
                _SaveFolderPath = null;
            }
        }

        private void ClassificationBtn_Click(object sender, EventArgs e)
        {
            int trainWeight = int.Parse(TrainTB.Text);
            int valWeight = int.Parse(ValidationTB.Text);
            int testWeight = int.Parse(TestTB.Text);

            //3. 원래 경로 내 하위 디렉토리마다 파일들을 가중치만큼 지정한 set 파일 내 폴더들로 이동
            serchSubDir(_Path);
            AutoSet(_Path, _SaveFolderPath, trainWeight, valWeight, testWeight);
        }

        // 숫자만 입력 받게 해주는 함수.
        private void OnlyNum(KeyPressEventArgs e)
        {
            if (!(char.IsDigit(e.KeyChar) || e.KeyChar == Convert.ToChar(Keys.Back)))
            {
                e.Handled = true;
            }
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
        private void cropWidth_KeyPress(object sender, KeyPressEventArgs e)
        {
            OnlyNum(e);
        }
        private void cropHeight_KeyPress(object sender, KeyPressEventArgs e)
        {
            OnlyNum(e);
        }
        private void TrainTB_KeyPress(object sender, KeyPressEventArgs e)
        {
            OnlyNum(e);
        }
        private void ValidationTB_KeyPress(object sender, KeyPressEventArgs e)
        {
            OnlyNum(e);
        }
        private void TestTB_KeyPress(object sender, KeyPressEventArgs e)
        {
            OnlyNum(e);
        }
    }
}
