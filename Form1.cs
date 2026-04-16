namespace FileCompare
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void btnLeftDir_Click(object sender, EventArgs e)
        {
            try
            {
                // [1] 실행하고 싶은 코드 (오류가 날 가능성이 있는 곳)
                using (var dlg = new FolderBrowserDialog())
                {
                    dlg.Description = "폴더를 선택하세요.";
                    // 현재 텍스트박스에 있는 경로를 초기 선택 폴더로 설정
                    if (!string.IsNullOrWhiteSpace(txtLeftDir.Text) &&
                        Directory.Exists(txtLeftDir.Text))
                    {
                        dlg.SelectedPath = txtLeftDir.Text;
                    }
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        txtLeftDir.Text = dlg.SelectedPath;
                        PopulateListView(lvwLeftDir, dlg.SelectedPath);

                        // 반대편이 이미 선택되어 있으면 동시에 갱신(색상 일관성 유지)
                        if (!string.IsNullOrWhiteSpace(txtRightDir.Text) &&
                            Directory.Exists(txtRightDir.Text))
                        {
                            PopulateListView(lvwrightDir, txtRightDir.Text);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // [2] 오류가 발생했을 때 실행되는 코드
                Console.WriteLine("오류 발생: " + ex.Message);
            }
            finally
            {
                // [3] 오류와 상관없이 무조건 마지막에 실행되는 코드(생략 가능)
            }
        }

        private void btnRightDir_Click(object sender, EventArgs e)
        {
            try
            {
                // [1] 실행하고 싶은 코드 (오류가 날 가능성이 있는 곳)
                using (var dlg = new FolderBrowserDialog())
                {
                    dlg.Description = "폴더를 선택하세요.";
                    // 현재 텍스트박스에 있는 경로를 초기 선택 폴더로 설정
                    if (!string.IsNullOrWhiteSpace(txtRightDir.Text) &&
                        Directory.Exists(txtRightDir.Text))
                    {
                        dlg.SelectedPath = txtRightDir.Text;
                    }
                    if (dlg.ShowDialog() == DialogResult.OK)
                    {
                        txtRightDir.Text = dlg.SelectedPath;
                        PopulateListView(lvwrightDir, dlg.SelectedPath);

                        // 반대편이 이미 선택되어 있으면 동시에 갱신(색상 일관성 유지)
                        if (!string.IsNullOrWhiteSpace(txtLeftDir.Text) &&
                            Directory.Exists(txtLeftDir.Text))
                        {
                            PopulateListView(lvwLeftDir, txtLeftDir.Text);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                // [2] 오류가 발생했을 때 실행되는 코드
                Console.WriteLine("오류 발생: " + ex.Message);
            }
            finally
            {
                // [3] 오류와 상관없이 무조건 마지막에 실행되는 코드(생략 가능)
            }
        }

        private void btnCopyFromLeft_Click(object sender, EventArgs e)
        {

        }

        private void btnCopyFromRight_Click(object sender, EventArgs e)
        {

        }

        private void PopulateListView(ListView lv, string folderPath)
        {
            lv.BeginUpdate();
            lv.Items.Clear();
            try
            { // 폴더(디렉터리) 먼저 추가
                var dirs = Directory.EnumerateDirectories(folderPath)
                .Select(p => new DirectoryInfo(p)).OrderBy(d => d.Name);
                foreach (var d in dirs)
                {
                    var item = new ListViewItem(d.Name);
                    item.SubItems.Add("<DIR>");
                    item.SubItems.Add(d.LastWriteTime.ToString("g"));
                    lv.Items.Add(item);
                }
                // 파일 추가
                var files = Directory.EnumerateFiles(folderPath)
                .Select(p => new FileInfo(p))
                .OrderBy(f => f.Name);

                // 비교 대상 폴더 결정 (반대편)
                string otherFolder = null;
                if (lv == lvwLeftDir)
                {
                    otherFolder = txtRightDir.Text;
                }
                else if (lv == lvwrightDir)
                {
                    otherFolder = txtLeftDir.Text;
                }

                foreach (var f in files)
                {
                    var item = new ListViewItem(f.Name);
                    item.SubItems.Add(f.Length.ToString("N0") + " 바이트");
                    item.SubItems.Add(f.LastWriteTime.ToString("g"));

                    // 상태 결정 및 색상 적용
                    FileInfo rf = null;
                    if (!string.IsNullOrWhiteSpace(otherFolder) && Directory.Exists(otherFolder))
                    {
                        var otherPath = Path.Combine(otherFolder, f.Name);
                        if (File.Exists(otherPath))
                        {
                            rf = new FileInfo(otherPath);
                        }
                    }

                    if (rf != null)
                    {
                        // 파일이름이 같고 크기가 같으며 수정시간도 같으면 동일 파일 -> 검은색
                        if (f.Length == rf.Length && f.LastWriteTime == rf.LastWriteTime)
                        {
                            item.ForeColor = Color.Black; // 동일
                        }
                        else if (f.Length == rf.Length) // 같은 파일(크기 같음)인데 시간 차이
                        {
                            if (f.LastWriteTime > rf.LastWriteTime)
                            {
                                item.ForeColor = Color.Red; // 이쪽이 더 최신
                            }
                            else
                            {
                                item.ForeColor = Color.Gray; // 이쪽이 더 오래됨
                            }
                        }
                        else
                        {
                            // 파일 이름은 같지만 크기가 다르면 시간/내용이 다른 것으로 간주
                            // 시간 기준으로 더 최신/오래된 색상을 적용하거나 회색으로 표시할 수 있음.
                            if (f.LastWriteTime > rf.LastWriteTime)
                            {
                                item.ForeColor = Color.Red;
                            }
                            else
                            {
                                item.ForeColor = Color.Gray;
                            }
                        }
                    }
                    else
                    {
                        // 반대편에 없는 단독 파일 -> 보라색
                        item.ForeColor = Color.Purple;
                    }

                    lv.Items.Add(item);
                }

                // 컬럼 너비 자동 조정 (컨텐츠 기준)
                for (int i = 0; i < lv.Columns.Count; i++)
                {
                    lv.AutoResizeColumn(i,
                    ColumnHeaderAutoResizeStyle.ColumnContent);
                }

            }
            catch (DirectoryNotFoundException)
            {
                MessageBox.Show(this, "폴더를 찾을 수 없습니다.", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            catch (IOException ex)
            {
                MessageBox.Show(this, "입출력 오류: " + ex.Message, "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                // BeginUpdate()로 잠근 상태를 반드시 해제
                lv.EndUpdate();
            }
        }

        
    }
}
