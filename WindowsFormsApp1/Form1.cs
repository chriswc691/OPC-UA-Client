using System;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using Opc.UaFx;
using Opc.UaFx.Client;



namespace WindowsFormsApp1
{    
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();


        }

        public OpcClient opcClient;
        public static List<OpcClient> serverList = new List<OpcClient>();
        public static List<OpcNodeInfo> rootNode = new List<OpcNodeInfo>();
        TreeNode selectedServerNode;
        public static TreeNode selectedGroupNode;
        public List<bool> serverConnection = new List<bool>();
        List<String> serverName = new List<String>();
        int groupCount = 1;
        int listIndex = 0;
        public static String formKey;
        bool isSubscribed = false;
        Dictionary<String, ArrayList> folder = new Dictionary<String, ArrayList>();
        public static Dictionary<String, List<String>> tagNodeID = new Dictionary<String, List<String>>();
        Dictionary<String, ListViewItem> keyValuePairs = new Dictionary<String, ListViewItem>();


        private void toolsToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView1.SelectedItems.Count > 0) //判斷listview有被選中項
            {
                deleteToolStripMenuItem.Enabled = true;
                listIndex = this.listView1.SelectedItems[0].Index;

            }
            else
            {
                deleteToolStripMenuItem.Enabled = false;
            }
        }

        private void listView2_SelectedIndexChanged(object sender, EventArgs e)
        {

        }

        private void menuStrip1_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {

        }

        public void newAServerConnectionToolStripMenuItem_Click(object sender, EventArgs e)  //建立新連線部分(工具列)
        {
            Form2 f = new Form2();
            f.ShowDialog(this);

            if (f.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    Console.WriteLine(f.loginMethod);
                    opcClient = new OpcClient(f.url);
                    if (f.loginMethod == 1)            //檢查登入方式，1為帳密登入
                    {
                        opcClient.Security.UserIdentity = new OpcClientIdentity(f.user, f.password);

                    }

                    else if (f.loginMethod == 0)      //檢查登入方式，0為匿名登入
                    {
                        opcClient.Security.UserIdentity = null;
                    }

                    opcClient.StateChanged += OpcClient_StateChanged;
                    opcClient.Connect();

                    MessageBox.Show("已成功連線");
                    serverConnection.Add(true);
                    serverList.Add(opcClient);

                    int serverIndex = serverList.IndexOf(opcClient);

                    var node = opcClient.BrowseNode(OpcObjectTypes.ObjectsFolder);
                    rootNode.Add(node);

                    treeView1.Nodes.Add(node.DisplayName);                      //在Treeview上建立Server的節點
                    treeView1.Nodes[serverIndex].Text = f.url;
                    serverName.Add(treeView1.Nodes[serverIndex].Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }
            else if (f.DialogResult == System.Windows.Forms.DialogResult.Cancel)
            {

            }
        }

        private void OpcClient_StateChanged(object sender, OpcClientStateChangedEventArgs e)  //判斷連線狀態，用來做Connect/Disconnect動作
        {
            if (e.NewState == OpcClientState.Connected && selectedServerNode != null)
            {
                serverConnection[selectedServerNode.Index] = true;
            }
            else if (e.NewState == OpcClientState.Disconnected)
            {
                serverConnection[selectedServerNode.Index] = false;
            }
        }


        private void fToolStripMenuItem_Click(object sender, EventArgs e)
        {

        }

        private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)  //左側Treeview的選取後的動作
        {
            //選取第一層(Server)的動作
            if (e.Node.Level == 0)
            {
                newGroupsToolStripMenuItem.Enabled = true;                          //判斷一些按鈕邏輯
                newAGroupToolStripMenuItem.Enabled = true;

                newItemsToolStripMenuItem.Enabled = false;
                newItemsToolStripMenuItem1.Enabled = false;

                addServerToolStripMenuItem.Enabled = true;
                newAServerConnectionToolStripMenuItem.Enabled = true;

                renameToolStripMenuItem.Enabled = false;
                renameToolStripMenuItem1.Enabled = false;

                subscribeToolStripMenuItem.Enabled = false;
                subscribeToolStripMenuItem1.Enabled = false;

                selectedServerNode = e.Node;


                //判斷連線狀態
                if (serverConnection[selectedServerNode.Index] == true)
                {
                    connectToolStripMenuItem.Enabled = false;
                    disconnectToolStripMenuItem.Enabled = true;
                }
                else
                {
                    connectToolStripMenuItem.Enabled = true;
                    disconnectToolStripMenuItem.Enabled = false;
                }
            }

            //選取第二層(Group)的動作
            else if (e.Node.Level == 1)
            {
                newItemsToolStripMenuItem.Enabled = true;                     ////判斷一些按鈕邏輯
                newItemsToolStripMenuItem1.Enabled = true;

                newGroupsToolStripMenuItem.Enabled = false;
                newAGroupToolStripMenuItem.Enabled = false;

                addServerToolStripMenuItem.Enabled = false;
                newAServerConnectionToolStripMenuItem.Enabled = false;

                connectToolStripMenuItem.Enabled = false;
                disconnectToolStripMenuItem.Enabled = false;

                renameToolStripMenuItem.Enabled = true;
                renameToolStripMenuItem1.Enabled = true;

                subscribeToolStripMenuItem.Enabled = true;
                subscribeToolStripMenuItem1.Enabled = true;


                selectedGroupNode = e.Node;


                //判斷連線狀態
                if (serverConnection[selectedServerNode.Index] == true)
                {
                    newItemsToolStripMenuItem.Enabled = true;
                    newItemsToolStripMenuItem1.Enabled = true;
                }
                else
                {
                    newItemsToolStripMenuItem.Enabled = false;
                    newItemsToolStripMenuItem1.Enabled = false;
                }

                String dicKey = "S" + selectedGroupNode.Parent.Index + "G" + selectedGroupNode.Index;   //將選取的Group位置存成 S_G_ 的格式，之後當成key值存資料，ex: 由上而下的順序，第1個Server下的第1個Group，代號為S0G0。
                formKey = dicKey;
                Console.WriteLine(dicKey);
                
                try                 //一般讀取(未訂閱)
                {
                    listView1.Items.Clear();


                    foreach (string[] arr in folder[dicKey])
                    {
                        ListViewItem tags = new ListViewItem(arr);
                        listView1.Items.Add(tags);

                    }
                }
                catch (KeyNotFoundException)
                {
                    listView1.Items.Clear();
                }

                if (isSubscribed == true)        //當訂閱開啟時，換頁都會變成訂閱的狀態
                {
                    
                    String dKey = "S" + selectedGroupNode.Parent.Index + "G" + selectedGroupNode.Index;

                    try
                    {
                        string[] nodeIds = tagNodeID[dKey].ToArray();

                        // Create an (empty) subscription to which we will addd OpcMonitoredItems.
                        OpcSubscription subscription = serverList[selectedGroupNode.Parent.Index].SubscribeNodes();

                        int index = 0;
                        foreach (ListViewItem viewItem in listView1.Items)
                        {

                            // Create an OpcMonitoredItem for the NodeId.
                            var item = new OpcMonitoredItem(nodeIds[index], OpcAttribute.Value);
                            item.DataChangeReceived += HandleDataChanged;

                            item.Tag = index;

                            // Set a custom sampling interval on the 
                            // monitored item.
                            item.SamplingInterval = 500;

                            // Add the item to the subscription.
                            subscription.AddMonitoredItem(item);

                            keyValuePairs.Remove(nodeIds[index]);
                            keyValuePairs.Add(nodeIds[index], viewItem);
                            index += 1;
                        }
                        subscription.ApplyChanges();
                    }

                    catch(KeyNotFoundException)
                    {

                    }

                }


            }
            else
            {
                newItemsToolStripMenuItem.Enabled = false;                //判斷一些按鈕邏輯
                newItemsToolStripMenuItem1.Enabled = false;

                newGroupsToolStripMenuItem.Enabled = false;
                newAGroupToolStripMenuItem.Enabled = false;

                renameToolStripMenuItem.Enabled = false;
                renameToolStripMenuItem1.Enabled = false;

                subscribeToolStripMenuItem.Enabled = false;
                subscribeToolStripMenuItem1.Enabled = false;
            }
        }



        private void newItemsToolStripMenuItem_Click(object sender, EventArgs e)    //建立新item部分(工具列)
        {
            Form3 f3 = new Form3();
            f3.ShowDialog(this);

            if (f3.DialogResult == System.Windows.Forms.DialogResult.OK)   //從Form3抓值過來
            {
                int idIndex = 0;
                int nameIndex = 0;
                ArrayList data = new ArrayList();

                
                if (folder.ContainsKey("S" + selectedGroupNode.Parent.Index + "G" + selectedGroupNode.Index))        //如果此Group已存在key值的情況下
                {
                    foreach (OpcValue element in f3.selectedValue)
                    {
                        string[] showItem = new string[5];
                        showItem[0] = f3.tagName[nameIndex++];
                        showItem[1] = element.DataType.ToString();
                        showItem[2] = element.Value.ToString();
                        showItem[3] = element.ServerTimestamp.ToString();
                        showItem[4] = element.Status.ToString();

                       
                        ListViewItem item = new ListViewItem(showItem);     //在Listview印出資料
                        listView1.Items.Add(item);

                        folder["S" + selectedGroupNode.Parent.Index + "G" + selectedGroupNode.Index].Add(showItem);                       //將新資料存進Dictionary
                        tagNodeID["S" + selectedGroupNode.Parent.Index + "G" + selectedGroupNode.Index].Add(f3.tagID[idIndex++]);         //將新資料的NodeID存進Dictionary
                    }

                }
                else
                {
                    foreach (OpcValue element in f3.selectedValue)
                    {
                        string[] showItem = new string[5];
                        showItem[0] = f3.tagName[nameIndex++];
                        showItem[1] = element.DataType.ToString();
                        showItem[2] = element.Value.ToString();
                        showItem[3] = element.ServerTimestamp.ToString();
                        showItem[4] = element.Status.ToString();

                        data.Add(showItem);
                        ListViewItem item = new ListViewItem(showItem);    //在Listview印出資料
                        listView1.Items.Add(item);
                                                
                    }
                    folder.Add("S" + selectedGroupNode.Parent.Index + "G" + selectedGroupNode.Index, data);                //將資料存進Dictionary
                    tagNodeID.Add("S" + selectedGroupNode.Parent.Index + "G" + selectedGroupNode.Index, f3.tagID);         //資料的NodeID存進Dictionary
                }
            }
            else if (f3.DialogResult == System.Windows.Forms.DialogResult.Cancel)
            {

            }

        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void saveFileDialog1_FileOk(object sender, CancelEventArgs e)
        {
            saveFileDialog1.InitialDirectory = @"F:\";
            saveFileDialog1.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";
            //openFileDialog1.FileName = "文件名";


        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            openFileDialog1.InitialDirectory = @"C:\";//设置起始文件夹
            openFileDialog1.Filter = "Text Files (*.txt)|*.txt|All Files (*.*)|*.*";//设置文件筛选类型
            openFileDialog1.FileName = "";//设施初始的文件名为空
            openFileDialog1.CheckFileExists = true;//检查文件是否存在
            openFileDialog1.CheckPathExists = true;//检查路径是否存在

            DialogResult result = openFileDialog1.ShowDialog();//显示对话框接返回值
            if (result == DialogResult.OK)
            {
                //textBox_content.Text = RWStream.ReadFile(openFileDialog1.FileName);
            }
        }

        private void newGroupsToolStripMenuItem_Click(object sender, EventArgs e)         //新增Group功能(工具列)
        {
            selectedServerNode.Nodes.Add("Group" + groupCount.ToString());
            groupCount += 1;
            selectedServerNode.Expand();

        }

        private void addServerToolStripMenuItem_Click(object sender, EventArgs e)    //建立新連線部分(右鍵)
        {
            Form2 f = new Form2();
            f.ShowDialog(this);

            if (f.DialogResult == System.Windows.Forms.DialogResult.OK)
            {
                try
                {
                    Console.WriteLine(f.loginMethod);
                    opcClient = new OpcClient(f.url);
                    if (f.loginMethod == 1)            //檢查登入方式，1為帳密登入
                    {
                        opcClient.Security.UserIdentity = new OpcClientIdentity(f.user, f.password);

                    }

                    else if (f.loginMethod == 0)     //檢查登入方式，0為匿名登入
                    {
                        opcClient.Security.UserIdentity = null;
                    }

                    opcClient.StateChanged += OpcClient_StateChanged;
                    opcClient.Connect();

                    MessageBox.Show("已成功連線");
                    serverConnection.Add(true);
                    serverList.Add(opcClient);

                    int serverIndex = serverList.IndexOf(opcClient);

                    var node = opcClient.BrowseNode(OpcObjectTypes.ObjectsFolder);
                    rootNode.Add(node);

                    treeView1.Nodes.Add(node.DisplayName);                           //在Treeview上建立Server的節點
                    treeView1.Nodes[serverIndex].Text = f.url;
                    serverName.Add(treeView1.Nodes[serverIndex].Text);
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message);
                }

            }
            else if (f.DialogResult == System.Windows.Forms.DialogResult.Cancel)
            {

            }
        }


        private void newItemsToolStripMenuItem1_Click(object sender, EventArgs e)   //建立新Item部分(右鍵)
        {
            Form3 f3 = new Form3();
            f3.ShowDialog(this);

            if (f3.DialogResult == System.Windows.Forms.DialogResult.OK)   //從Form3抓值過來
            {
                int idIndex = 0;
                int nameIndex = 0;
                ArrayList data = new ArrayList();

                
                if (folder.ContainsKey("S" + selectedGroupNode.Parent.Index + "G" + selectedGroupNode.Index))        //如果此Group已存在key值的情況下
                {
                    foreach (OpcValue element in f3.selectedValue)
                    {
                        string[] showItem = new string[5];
                        showItem[0] = f3.tagName[nameIndex++];
                        showItem[1] = element.DataType.ToString();
                        showItem[2] = element.Value.ToString();
                        showItem[3] = element.ServerTimestamp.ToString();
                        showItem[4] = element.Status.ToString();
                       
                        ListViewItem item = new ListViewItem(showItem);     //在Listview印出資料
                        listView1.Items.Add(item);

                        folder["S" + selectedGroupNode.Parent.Index + "G" + selectedGroupNode.Index].Add(showItem);                       //將新資料存進Dictionary
                        tagNodeID["S" + selectedGroupNode.Parent.Index + "G" + selectedGroupNode.Index].Add(f3.tagID[idIndex++]);         //將新資料的NodeID存進Dictionary
                    }

                }
                else
                {
                    foreach (OpcValue element in f3.selectedValue)
                    {
                        string[] showItem = new string[5];
                        showItem[0] = f3.tagName[nameIndex++];
                        showItem[1] = element.DataType.ToString();
                        showItem[2] = element.Value.ToString();
                        showItem[3] = element.ServerTimestamp.ToString();
                        showItem[4] = element.Status.ToString();

                        data.Add(showItem);
                        ListViewItem item = new ListViewItem(showItem);    //在Listview印出資料
                        listView1.Items.Add(item);
                                                
                    }
                    folder.Add("S" + selectedGroupNode.Parent.Index + "G" + selectedGroupNode.Index, data);                //將資料存進Dictionary
                    tagNodeID.Add("S" + selectedGroupNode.Parent.Index + "G" + selectedGroupNode.Index, f3.tagID);         //資料的NodeID存進Dictionary
                }
            }
            else if (f3.DialogResult == System.Windows.Forms.DialogResult.Cancel)
            {

            }
        }

       
        public void HandleDataChanged(object sender, OpcDataChangeReceivedEventArgs e)
        {
            // Your code to execute on each data change.
            // The 'sender' variable contains the OpcMonitoredItem with the NodeId.
            OpcMonitoredItem item = (OpcMonitoredItem)sender;

            ListViewItem lviH = keyValuePairs[item.NodeId.ToString()];           //將NodeId做為Key，存取ListViewItem
            lviH.SubItems[2].Text = e.Item.Value.ToString();                     //更改Value的Text，達成訂閱的效果

        }


        private void newAGroupToolStripMenuItem_Click(object sender, EventArgs e)    //新增Group功能(右鍵)
        {
            selectedServerNode.Nodes.Add("Group" + groupCount.ToString());
            groupCount += 1;
            selectedServerNode.Expand();

        }

        private void treeView1_AfterLabelEdit(object sender, NodeLabelEditEventArgs e)
        {
            treeView1.LabelEdit = false;
        }

        private void connectToolStripMenuItem_Click(object sender, EventArgs e)      //選取已建立的連線Connect的方法
        {
            try
            {
                serverList[selectedServerNode.Index].Connect();
                MessageBox.Show("連線成功");
                treeView1.Nodes[selectedServerNode.Index].Text = serverName[selectedServerNode.Index];
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private void disconnectToolStripMenuItem_Click(object sender, EventArgs e)    //選取已建立的連線Disconnect的方法
        {
            serverList[selectedServerNode.Index].Disconnect();
            MessageBox.Show("已中斷連線");

            treeView1.Nodes[selectedServerNode.Index].Text += "(尚未連接)";

        }


        private void renameToolStripMenuItem1_Click(object sender, EventArgs e)  //改Group名的功能(右鍵)
        {

            treeView1.LabelEdit = true;
            selectedGroupNode.BeginEdit();
        }

        private void renameToolStripMenuItem_Click(object sender, EventArgs e)   //改Group名的功能(工具列)
        {

            treeView1.LabelEdit = true;
            selectedGroupNode.BeginEdit();
        }

        private void deleteToolStripMenuItem_Click(object sender, EventArgs e)         //刪除Tag功能
        {
            listView1.Items[listIndex].Remove();
            folder["S" + selectedGroupNode.Parent.Index + "G" + selectedGroupNode.Index].RemoveAt(listIndex);
            tagNodeID["S" + selectedGroupNode.Parent.Index + "G" + selectedGroupNode.Index].RemoveAt(listIndex);

        }

        private void subscribeToolStripMenuItem_Click(object sender, EventArgs e)                           //訂閱功能(工具列)
        {
            String dKey = "S" + selectedGroupNode.Parent.Index + "G" + selectedGroupNode.Index;

            string[] nodeIds = tagNodeID[dKey].ToArray();                                                   //用Group代號去Dictionary找資料

            // Create an (empty) subscription to which we will addd OpcMonitoredItems.
            OpcSubscription subscription = serverList[selectedGroupNode.Parent.Index].SubscribeNodes();
            
            isSubscribed = true;

            int index = 0;
            foreach (ListViewItem viewItem in listView1.Items)
            {
                
                // Create an OpcMonitoredItem for the NodeId.
                var item = new OpcMonitoredItem(nodeIds[index], OpcAttribute.Value);
                item.DataChangeReceived += HandleDataChanged;

                // You can set your own values on the "Tag" property
                // that allows you to identify the source later.
                item.Tag = index;

                // Set a custom sampling interval on the 
                // monitored item.
                item.SamplingInterval = 500;

                // Add the item to the subscription.
                subscription.AddMonitoredItem(item);

                keyValuePairs.Remove(nodeIds[index]);
                keyValuePairs.Add(nodeIds[index], viewItem);
                index += 1;
                
            }

            subscription.ApplyChanges();
                
        }

        private void subscribeToolStripMenuItem1_Click(object sender, EventArgs e)                            //訂閱功能(右鍵)
        {
            String dKey = "S" + selectedGroupNode.Parent.Index + "G" + selectedGroupNode.Index;

            string[] nodeIds = tagNodeID[dKey].ToArray();                                                     //用Group代號去Dictionary找資料

            // Create an (empty) subscription to which we will addd OpcMonitoredItems.
            OpcSubscription subscription = serverList[selectedGroupNode.Parent.Index].SubscribeNodes();

            isSubscribed = true;

            int index = 0;
            foreach (ListViewItem viewItem in listView1.Items)
            {

                // Create an OpcMonitoredItem for the NodeId.
                var item = new OpcMonitoredItem(nodeIds[index], OpcAttribute.Value);
                item.DataChangeReceived += HandleDataChanged;

                // You can set your own values on the "Tag" property
                // that allows you to identify the source later.
                item.Tag = index;

                // Set a custom sampling interval on the 
                // monitored item.
                item.SamplingInterval = 500;

                // Add the item to the subscription.
                subscription.AddMonitoredItem(item);

                keyValuePairs.Remove(nodeIds[index]);
                keyValuePairs.Add(nodeIds[index], viewItem);
                index += 1;
                
            }

            subscription.ApplyChanges();

        }
    }
}
