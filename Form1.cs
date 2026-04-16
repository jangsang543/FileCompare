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
            try
            {
                // 왼쪽 -> 오른쪽으로 선택한 파일/폴더 복사
                if (string.IsNullOrWhiteSpace(txtLeftDir.Text) || !Directory.Exists(txtLeftDir.Text))
                {
                    MessageBox.Show(this, "왼쪽 폴더가 선택되어 있지 않습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtRightDir.Text) || !Directory.Exists(txtRightDir.Text))
                {
                    MessageBox.Show(this, "오른쪽 폴더가 선택되어 있지 않습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var selected = lvwLeftDir.SelectedItems;
                if (selected.Count == 0)
                {
                    MessageBox.Show(this, "복사할 항목을 선택하세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int copied = 0;
                int skippedDir = 0;
                foreach (ListViewItem it in selected)
                {
                    string name = it.Text;
                    string srcPath = Path.Combine(txtLeftDir.Text, name);
                    string dstPath = Path.Combine(txtRightDir.Text, name);

                    // 디렉터리(하위폴더)를 하나의 항목처럼 처리
                    if (it.SubItems.Count > 1 && it.SubItems[1].Text == "<DIR>")
                    {
                        // 대상 디렉터리가 존재하고 대상이 더 최신이면 사용자 확인 (폴더 단위로 묶음 처리)
                        if (Directory.Exists(dstPath))
                        {
                            var sDir = new DirectoryInfo(srcPath);
                            var dDir = new DirectoryInfo(dstPath);
                            if (dDir.LastWriteTime > sDir.LastWriteTime)
                            {
                                string msg = "대상에 동일한 이름의 파일이 이미 있습니다.\r\n대상 파일이 더 신규 파일입니다. 덮어쓰시겠습니까?\r\n\r\n원본: " + srcPath + "\r\n대상: " + dstPath;
                                var dr = MessageBox.Show(this, msg, "덮어쓰기 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                                if (dr != DialogResult.Yes)
                                {
                                    skippedDir++;
                                    continue; // 건너뜀
                                }
                            }
                        }

                        try
                        {
                            // 재귀 복사 (하위 폴더와 파일 포함), 기존 파일 덮어쓰기
                            CopyDirectoryRecursive(srcPath, dstPath);
                            copied++; // 디렉터리 단위로 카운트
                        }
                        catch (Exception exDir)
                        {
                            MessageBox.Show(this, $"폴더 복사 실패: {name}\n{exDir.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }

                        continue;
                    }

                    // 파일 복사 처리
                    try
                    {
                        // 대상 파일이 존재하고 대상이 더 최신인 경우 사용자에게 확인 (파일 단위)
                        if (File.Exists(dstPath))
                        {
                            var sInfo = new FileInfo(srcPath);
                            var dInfo = new FileInfo(dstPath);
                            if (dInfo.LastWriteTime > sInfo.LastWriteTime)
                            {
                                string msg = "대상에 동일한 이름의 파일이 이미 있습니다.\r\n대상 파일이 더 신규 파일입니다. 덮어쓰시겠습니까?\r\n\r\n원본: " + srcPath + "\r\n대상: " + dstPath;
                                var dr = MessageBox.Show(this, msg, "덮어쓰기 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                                if (dr != DialogResult.Yes)
                                {
                                    continue; // 건너뜀
                                }
                            }
                        }

                        File.Copy(srcPath, dstPath, true); // 덮어쓰기 허용
                        copied++;
                    }
                    catch (Exception exFile)
                    {
                        MessageBox.Show(this, $"파일 복사 실패: {name}\n{exFile.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                // 복사 후 양쪽 리스트뷰 갱신(일관된 색상 표시를 위해)
                PopulateListView(lvwLeftDir, txtLeftDir.Text);
                PopulateListView(lvwrightDir, txtRightDir.Text);

                // 요약 알림 (필요하면 표시)
                
            }
            catch (Exception ex)
            {
                Console.WriteLine("오류 발생: " + ex.Message);
            }
        }

        private void btnCopyFromRight_Click(object sender, EventArgs e)
        {
            try
            {
                // 오른쪽 -> 왼쪽으로 선택한 파일/폴더 복사
                if (string.IsNullOrWhiteSpace(txtRightDir.Text) || !Directory.Exists(txtRightDir.Text))
                {
                    MessageBox.Show(this, "오른쪽 폴더가 선택되어 있지 않습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }
                if (string.IsNullOrWhiteSpace(txtLeftDir.Text) || !Directory.Exists(txtLeftDir.Text))
                {
                    MessageBox.Show(this, "왼쪽 폴더가 선택되어 있지 않습니다.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                var selected = lvwrightDir.SelectedItems;
                if (selected.Count == 0)
                {
                    MessageBox.Show(this, "복사할 항목을 선택하세요.", "알림", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    return;
                }

                int copied = 0;
                int skippedDir = 0;
                foreach (ListViewItem it in selected)
                {
                    string name = it.Text;
                    string srcPath = Path.Combine(txtRightDir.Text, name);
                    string dstPath = Path.Combine(txtLeftDir.Text, name);

                    // 폴더 단위 처리
                    if (it.SubItems.Count > 1 && it.SubItems[1].Text == "<DIR>")
                    {
                        if (Directory.Exists(dstPath))
                        {
                            var sDir = new DirectoryInfo(srcPath);
                            var dDir = new DirectoryInfo(dstPath);
                            if (dDir.LastWriteTime > sDir.LastWriteTime)
                            {
                                string msg = "대상에 동일한 이름의 파일이 이미 있습니다.\r\n대상 파일이 더 신규 파일입니다. 덮어쓰시겠습니까?\r\n\r\n원본: " + srcPath + "\r\n대상: " + dstPath;
                                var dr = MessageBox.Show(this, msg, "덮어쓰기 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                                if (dr != DialogResult.Yes)
                                {
                                    skippedDir++;
                                    continue; // 건너뜀
                                }
                            }
                        }

                        try
                        {
                            CopyDirectoryRecursive(srcPath, dstPath);
                            copied++;
                        }
                        catch (Exception exDir)
                        {
                            MessageBox.Show(this, $"폴더 복사 실패: {name}\n{exDir.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        }

                        continue;
                    }

                    // 파일 단위 처리
                    try
                    {
                        if (File.Exists(dstPath))
                        {
                            var sInfo = new FileInfo(srcPath);
                            var dInfo = new FileInfo(dstPath);
                            if (dInfo.LastWriteTime > sInfo.LastWriteTime)
                            {
                                string msg = "대상에 동일한 이름의 파일이 이미 있습니다.\r\n대상 파일이 더 신규 파일입니다. 덮어쓰시겠습니까?\r\n\r\n원본: " + srcPath + "\r\n대상: " + dstPath;
                                var dr = MessageBox.Show(this, msg, "덮어쓰기 확인", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                                if (dr != DialogResult.Yes)
                                {
                                    continue; // 건너뜀
                                }
                            }
                        }

                        File.Copy(srcPath, dstPath, true); // 덮어쓰기 허용
                        copied++;
                    }
                    catch (Exception exFile)
                    {
                        MessageBox.Show(this, $"파일 복사 실패: {name}\n{exFile.Message}", "오류", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }

                // 복사 후 양쪽 리스트뷰 갱신
                PopulateListView(lvwLeftDir, txtLeftDir.Text);
                PopulateListView(lvwrightDir, txtRightDir.Text);

                // 요약 메시지 제거: 더 이상 복사 요약을 표시하지 않음
            }
            catch (Exception ex)
            {
                Console.WriteLine("오류 발생: " + ex.Message);
            }
        }

        // 하위 폴더를 재귀적으로 복사 (덮어쓰기 허용)
        private void CopyDirectoryRecursive(string sourceDirName, string destDirName)
        {
            var sourceDir = new DirectoryInfo(sourceDirName);
            if (!sourceDir.Exists)
                throw new DirectoryNotFoundException("Source directory not found: " + sourceDirName);

            // 대상 디렉터리 생성
            Directory.CreateDirectory(destDirName);

            // 파일 복사
            foreach (var file in sourceDir.GetFiles())
            {
                string targetFilePath = Path.Combine(destDirName, file.Name);
                // 파일이 존재하더라도 폴더 단위에서 이미 사용자가 허용했으므로 덮어쓰기
                file.CopyTo(targetFilePath, true);
            }

            // 하위 디렉터리 재귀 복사
            foreach (var subdir in sourceDir.GetDirectories())
            {
                string targetSubDir = Path.Combine(destDirName, subdir.Name);
                CopyDirectoryRecursive(subdir.FullName, targetSubDir);
            }

            // 복사 후 대상 디렉터리의 최종 수정 시간은 소스 디렉터리의 수정 시간으로 맞춰둠
            try
            {
                Directory.SetLastWriteTime(destDirName, sourceDir.LastWriteTime);
            }
            catch
            {
                // 무시
            }
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

                    // 폴더(하위폴더)를 하나의 항목으로 처리하여 색상 결정
                    if (!string.IsNullOrWhiteSpace(otherFolder) && Directory.Exists(otherFolder))
                    {
                        var otherPath = Path.Combine(otherFolder, d.Name);
                        if (Directory.Exists(otherPath))
                        {
                            var od = new DirectoryInfo(otherPath);
                            if (d.LastWriteTime == od.LastWriteTime)
                            {
                                item.ForeColor = Color.Black; // 동일
                            }
                            else if (d.LastWriteTime > od.LastWriteTime)
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
                            item.ForeColor = Color.Purple; // 반대편에 없음
                        }
                    }
                    else
                    {
                        item.ForeColor = Color.Purple; // 반대편이 비어있음
                    }

                    lv.Items.Add(item);
                }
                // 파일 추가
                var files = Directory.EnumerateFiles(folderPath)
                .Select(p => new FileInfo(p))
                .OrderBy(f => f.Name);

                // 비교 대상 폴더 결정 (반대편) - reused
                string otherFolderForFiles = null;
                if (lv == lvwLeftDir)
                {
                    otherFolderForFiles = txtRightDir.Text;
                }
                else if (lv == lvwrightDir)
                {
                    otherFolderForFiles = txtLeftDir.Text;
                }

                foreach (var f in files)
                {
                    var item = new ListViewItem(f.Name);
                    item.SubItems.Add(f.Length.ToString("N0") + " 바이트");
                    item.SubItems.Add(f.LastWriteTime.ToString("g"));

                    // 상태 결정 및 색상 적용
                    FileInfo rf = null;
                    if (!string.IsNullOrWhiteSpace(otherFolderForFiles) && Directory.Exists(otherFolderForFiles))
                    {
                        var otherPath = Path.Combine(otherFolderForFiles, f.Name);
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
