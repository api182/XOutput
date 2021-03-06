﻿using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace XOutput
{
    public partial class ControllerOptions : Form
    {
        ControllerDevice dev;
        public ControllerOptions(ControllerDevice device)
        {
            InitializeComponent();
            dev = device;
            int ind = 0;

            foreach (MultiLevelComboBox m in this.Controls.OfType<MultiLevelComboBox>())
            {
                //Tag structure: [Type, Number, Index]
                m.Items[0] = getBindingText(ind); //Change combobox text according to saved binding
                m.addOption("Disabled",
                    tag: new byte[] { 255, 0, (byte)ind });
                //m.addOption("Detect",
                //    tag: new byte[] { 254, 0, (byte)ind });
                ToolStripMenuItem axes = m.addMenu("Axes");
                ToolStripMenuItem buttons = m.addMenu("Buttons");
                ToolStripMenuItem dpads = m.addMenu("D-Pads");
                ToolStripMenuItem iaxes = m.addMenu("Inverted Axes", axes);
                ToolStripMenuItem haxes = m.addMenu("Half Axes", axes);
                ToolStripMenuItem ihaxes = m.addMenu("Inverted Half Axes", axes);
                for (int i = 1; i <= dev.joystick.Capabilities.ButtonCount; i++)
                {
                    m.addOption("Button " + i.ToString(), buttons,
                        new byte[] { 0, (byte)(i - 1), (byte)ind });
                }
                for (int i = 1; i <= dev.joystick.Capabilities.PovCount; i++)
                {
                    m.addOption("D-Pad " + i.ToString() + " Up", dpads,
                        new byte[] { 32, (byte)(i - 1), (byte)ind });
                    m.addOption("D-Pad " + i.ToString() + " Down", dpads,
                        new byte[] { 33, (byte)(i - 1), (byte)ind });
                    m.addOption("D-Pad " + i.ToString() + " Left", dpads,
                        new byte[] { 34, (byte)(i - 1), (byte)ind });
                    m.addOption("D-Pad " + i.ToString() + " Right", dpads,
                        new byte[] { 35, (byte)(i - 1), (byte)ind });
                }
                for (int i = 0; i <= dev.analogs.Length - 1; i++)
                {
                    if (dev.analogs[i] != 0)
                    {
                        int ii = i + 1;

                        m.addOption("Axis " + ii.ToString(), axes,
                            new byte[] { 16, (byte)(i), (byte)ind });
                        m.addOption("IAxis " + ii.ToString(), iaxes,
                            new byte[] { 17, (byte)(i), (byte)ind });
                        m.addOption("HAxis" + ii.ToString(), haxes,
                            new byte[] { 18, (byte)(i), (byte)ind });
                        m.addOption("IHAxis" + ii.ToString(), ihaxes,
                            new byte[] { 19, (byte)(i), (byte)ind });
                    }

                }
                m.SelectionChangeCommitted += new System.EventHandler(SelectionChanged);
                ind++;
            }

            this.numericCompensation.Tag = new byte[] { 253, (byte)(0), (byte)ind };

            int outVal = 0;
            int.TryParse(getBindingText(ind), out outVal);

            this.numericCompensation.Value = outVal > 0 ? (outVal) : 0;
        }

        private string getBindingText(int i)
        {
            if (dev.mapping[i * 2] == 255)
            {
                return "Disabled";
            }
            else if (dev.mapping[i * 2] == 253)
            {
                return (dev.mapping[((i * 2) + 1)].ToString());
            }
            byte subType = (byte)(dev.mapping[i * 2] & 0x0F);
            byte type = (byte)((dev.mapping[i * 2] & 0xF0) >> 4);
            byte num = (byte)(dev.mapping[(i * 2) + 1] + 1);
            string[] typeString = new string[] { "Button {0}", "{1}Axis {0}", "D-Pad {0} {2}" };
            string[] axesString = new string[] { "", "I", "H", "IH" };
            string[] dpadString = new string[] { "Up", "Down", "Left", "Right" };
            return string.Format(typeString[type], num, axesString[subType], dpadString[subType]);
        }

        private void SelectionChanged(object sender, EventArgs e)
        {
            ToolStripMenuItem i = (ToolStripMenuItem)sender;
            byte[] b = (byte[])i.Tag;
            if (b[0] == 254)
            {
                //start thread
                return;
            }
            dev.mapping[b[2] * 2] = b[0];
            dev.mapping[(b[2] * 2) + 1] = b[1];
            dev.Save();
        }

        private void onClose(object sender, EventArgs e)
        {
            dev.Save();
        }

        private void numericCompensation_ValueChanged(object sender, EventArgs e)
        {
            NumericUpDown i = (NumericUpDown)sender;
            byte[] b = (byte[])i.Tag;
            dev.mapping[b[2] * 2] = b[0];
            dev.mapping[(b[2] * 2) + 1] = (byte)(i.Value);
            dev.Save();
        }
    }
}
