using Opc.UaFx;
using Opc.UaFx.Client;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace WindowsFormsApp1
{
    public partial class Form3 : Form
    {
        public Form3()
        {
            InitializeComponent();
            button1.DialogResult = System.Windows.Forms.DialogResult.OK;//設定button1為OK
            button2.DialogResult = System.Windows.Forms.DialogResult.Cancel;//設定button為Cancel

                        
            treeView1.Nodes.Add(Form1.rootNode[serverIndex].DisplayName);   //加入母節點
            TreeNode tn = treeView1.Nodes[0];
            Browse(Form1.rootNode[serverIndex], tn);                        //先讀取第一層
        }

        String selectedTag;
        String selectedTagName;
        TreeNode selectedNode;
        public List<OpcValue> selectedValue = new List<OpcValue>();
        public List<String> tagName = new List<String>();
        public List<String> tagID = new List<String>();
        int listIndex = 0;
        public int serverIndex = Form1.selectedGroupNode.Parent.Index;


        private void Browse(OpcNodeInfo node, TreeNode tn)         //讀取單層(下一層)的方法
        {

            foreach (var childNode in node.Children())
            {

                TreeNode sub_tn = tn.Nodes.Add(childNode.DisplayName);

                sub_tn.Tag = childNode.NodeId.ToString();

            }

        }

        public void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (e.Node.Level > 0)                       //如果選擇的層是第0層以後
            {
                button3.Enabled = true;

                selectedTag = e.Node.Tag.ToString();
                selectedTagName = e.Node.Text;
                selectedNode = e.Node;
                OpcValue opcValue = Form1.serverList[serverIndex].ReadNode(selectedTag);


                var browse = new OpcBrowseNode(
                    nodeId: OpcNodeId.Parse(selectedTag),
                    degree: OpcBrowseNodeDegree.Generation);


                var node = Form1.serverList[serverIndex].BrowseNode(browse);

                foreach (var childNode in node.Children())    //讀取下一層
                {
                    TreeNode sub_tn = selectedNode.Nodes.Add(childNode.DisplayName);

                    sub_tn.Tag = childNode.NodeId.ToString();

                }

                if (opcValue.Value == null)       //如果選的資料夾(沒有值)，加入全部點的按鈕(button5)才會亮起
                {
                    button5.Enabled = true;
                }
                else
                {
                    button5.Enabled = false;
                }

            }
            else
            {

                button3.Enabled = false;
            }

        }

        private void listView1_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (this.listView1.SelectedItems.Count > 0)                //判斷listview有被選中項
            {
                listIndex = this.listView1.SelectedItems[0].Index;
                button4.Enabled = true;
            }
            else
            {
                button4.Enabled = false;
            }
        }

        private void button3_Click(object sender, EventArgs e)              //加入單個Tag按鈕
        {
            
            OpcValue opcValue = Form1.serverList[serverIndex].ReadNode(selectedTag);
            bool isAdded = false;

            foreach (String st in tagID)                                    //判斷是否有重複加到點(同頁)
            {
                if (selectedTag == st)
                {
                    isAdded = true;
                }

            }

            if (Form1.tagNodeID.ContainsKey(Form1.formKey) == true)         //判斷是否有重複加到點(Form1的Tag)
            {
                foreach (String id in Form1.tagNodeID[Form1.formKey])
                {
                    if (selectedTag == id)
                    {
                        isAdded = true;
                    }

                }
            }

            if (opcValue.Value == null)
            {
                MessageBox.Show("所選節點的值為空");

            }
            else if (isAdded)
            {
                MessageBox.Show("所選節點已存在");
            }

            else
            {
                //將選擇的Tags需用到的資料加入List，供Form1使用

                listView1.Items.Add(new ListViewItem(new string[] { selectedTagName, opcValue.DataType.ToString(), opcValue.Value.ToString(), opcValue.ServerTimestamp.ToString(), opcValue.Status.ToString() }));
                selectedValue.Add(opcValue);
                tagName.Add(selectedTagName);
                tagID.Add(selectedTag);

            }
        }

        private void button4_Click(object sender, EventArgs e)            //刪除Tag
        {
            listView1.Items[listIndex].Remove();
            selectedValue.RemoveAt(listIndex);
            tagName.RemoveAt(listIndex);
            tagID.RemoveAt(listIndex);

        }

        private void button5_Click(object sender, EventArgs e)                     //加入全部點的按鈕
        {
            var browse = new OpcBrowseNode(
                    nodeId: OpcNodeId.Parse(selectedTag),
                    degree: OpcBrowseNodeDegree.Generation);

            var node = Form1.serverList[serverIndex].BrowseNode(browse);
            List<OpcNodeInfo> all = new List<OpcNodeInfo>();

            Add_All(node, all);                                                    //呼叫Add_All方法

            foreach (OpcNodeInfo tree in all)
            {
                OpcValue opcValue = Form1.serverList[serverIndex].ReadNode(tree.NodeId);

                
                listView1.Items.Add(new ListViewItem(new string[] { tree.DisplayName.ToString(), opcValue.DataType.ToString(), opcValue.Value.ToString(), opcValue.ServerTimestamp.ToString(), opcValue.Status.ToString() }));
                selectedValue.Add(opcValue);
                tagName.Add(tree.DisplayName.ToString());
                tagID.Add(tree.NodeId.ToString());
            }
        }

        private void Add_All(OpcNodeInfo node, List<OpcNodeInfo> st)                                     //遞迴抓全部Tag存進List的方法
        {
            
            foreach (var childNode in node.Children())
            {
                OpcValue opcValue = Form1.serverList[serverIndex].ReadNode(childNode.NodeId);

               
                if (childNode.Category == OpcNodeCategory.Variable && opcValue.Value != null)           //只抓有Value的子節點
                {
                    st.Add(childNode);

                }

                if (childNode.Category == OpcNodeCategory.Variable)                                     //避免抓到Tag下最底層的屬性
                {
                    continue;
                }

                Add_All(childNode, st);
            }
        }
    }
}
