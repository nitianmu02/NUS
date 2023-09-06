using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.VisualBasic;
using UMC.FA.ECS.WinSECSMarshalObj;
using UMC.FA.ECS.EAP;
using UMC.FA.ECS.ED;
using CommonConstants = UMC.FA.Kit.ED.Utility.modConstants;
using UMC.FA.ECS;
using UMC.FA.Specific.ED.Kla8900;
using UMC.FA.Kit.ED.Utility;


                        nItem3.Value = "SubstrateCount";

                        nItem3 = nItem2.AddNew("2");
                        nItem3.Name = "CATTRDATA";
                        nItem3.Description = "Carrier Attribute Value";
                        nItem3.Format = SECS_FORMAT.U1;
                        nItem3.NLB = 1;
                        subStrCount = 0;

                        for (count = 0; count <= slotMapArray.GetUpperBound(0); count++)
                        {
                            if (slotMapArray[count] == 3)
                            {
                                subStrCount = subStrCount + 1;
                            }
                        }

                        nItem3.Value = subStrCount;

                        // --------------------------------
                        nItem2 = nItem.AddNew("3");    // Carrier Attribute(4)
                        nItem2.Name = "L2";
                        nItem2.Description = "Carrier Attribute Item";
                        nItem2.Format = SECS_FORMAT.LIST;
                        nItem2.NLB = 1;

                        nItem3 = nItem2.AddNew("1");
                        nItem3.Name = "CATTRID";
                        nItem3.Description = "Carrier Attribute";
                        nItem3.Format = SECS_FORMAT.ASCII;
                        nItem3.NLB = 1;
                        nItem3.Value = "ContentMap";

                        nItem3 = nItem2.AddNew("2");
                        nItem3.Name = "CATTRDATA";
                        nItem3.Description = "Carrier Attribute Value";
                        nItem3.Format = SECS_FORMAT.LIST;
                        nItem3.NLB = 1;
                        
                        for (count = 0; count <= slotMapArray.GetUpperBound(0); count++)
                        {
                            nItem4 = nItem3.AddNew((count + 1).ToString());
                            nItem4.Name = "L2";
                            nItem4.Description = "Slot" + (count + 1);
                            nItem4.Format = SECS_FORMAT.LIST;
                            nItem4.NLB = 1;

                            nItem5 = nItem4.AddNew("1");
                            nItem5.Name = "LotID";
                            nItem5.Description = "";
                            nItem5.Format = SECS_FORMAT.ASCII;
                            nItem5.NLB = 1;
                            nItem5.Value = lotArray[count];

                            nItem5 = nItem4.AddNew("2");
                            nItem5.Name = "SubstrateID";
                            nItem5.Description = "";
                            nItem5.Format = SECS_FORMAT.ASCII;
                            nItem5.NLB = 1;
                            nItem5.Value = componentArray[count];
                        }

                        //    With .AddNew(4) 'Carrier Attribute(3)
                        //        .Name = "L2"
                        //        .Description = "Carrier Attribute Item"
                        //        .Format = wsFormatList
                        //        .NLB = 1
                        //        With .AddNew(1)
                        //            .Name = "CATTRID"
                        //            .Description = "Carrier Attribute"
                        //            .Format = wsFormatAscii
                        //            .NLB = 1
                        //            .value = "SlotMap"
                        //        End With
                        //        With .AddNew(2)
                        //            .Name = "CATTRDATA"
                        //            .Description = "Carrier Attribute Value"
                        //            .Format = wsFormatList
                        //            .NLB = 1
                        //            For iCnt = 1 To UBound(iSlotMap) 'Start from 1
                        //                With .AddNew(iCnt)
                        //                    .Name = "Slot"
                        //                    .Description = "Slot" & iCnt
                        //                    .Format = wsFormatU1
                        //                    .NLB = 1
                        //                    .value = iSlotMap(iCnt)
                        //                End With
                        //            Next iCnt
                        //        End With
                        //    End With

                        nItem2 = nItem.AddNew("4");    // Carrier Attribute(5)
                        nItem2.Name = "L2";
                        nItem2.Description = "Carrier Attribute Item";
                        nItem2.Format = SECS_FORMAT.LIST;
                        nItem2.NLB = 1;

                        nItem3 = nItem2.AddNew("1");    
                        nItem3.Name = "CATTRID";
                        nItem3.Description = "Carrier Attribute";
                        nItem3.Format = SECS_FORMAT.ASCII;
                        nItem3.NLB = 1;
                        nItem3.Value = "Usage";

                        nItem3 = nItem2.AddNew("2");      
                        nItem3.Name = "CATTRID";
                        nItem3.Description = "Carrier Attribute";
                        nItem3.Format = SECS_FORMAT.ASCII;
                        nItem3.NLB = 1;
                        nItem3.Value = "PRODUCT";
                    }
                }

                result = secsTrans;
            }
            finally
            {
                // ------
                // Cleaning Up
                // ------
                secsTrans = null;                
                pItem = null;
                nItem = null;
            }

            return result;
        }
        public static SECSTransaction BuildS3F23_AccessMode(this AbstractDriverAction action, VfeiMessage vfei)
        {
            // Weicher: Added to support change access mode from AUI (31 Dec 2004)

            // S3F23 (PGAR) - Port Group Action Request
            //      < L3
            //           < A PGRPACTION = 'ChangeAccess' - Port Group Action >
            //           < A PORTGRPNAME = '1' - Port Group Name >
            //           < L1
            //                < L2
            //                     < A PARANAME = 'AccessMode' >
            //                     < U1 PARAMVAL = 0 >
            //                >
            //           >
            //      >

            string procedureID = String.Empty;
            procedureID = "BuildS3F23_AccessMode";
            SECSTransaction result = null;
            SECSTransaction secsTrans = null;
            SECSMessage msg = null;
            SECSItem pItem = null;
            SECSItem nItem = null;
            int accessMode = -1;
            Triplet temTriplet;
            string xPath = "0";
            string xPath2 = "0~2";
            string xPath3 = "0~2~0";

            try
            {
                secsTrans = action.Parent.NewTransaction("S3F23");

                msg = secsTrans.Primary;
                msg.Root.Name = "HCS";
                msg.Root.Description = "Host command send";
                msg.Function = 23;
                msg.Stream = 3;

                if (msg.Root.ItemCount > 0)
                {
                    while (msg.Root.ItemCount > 0)
                    {
                        pItem = action.Parent.GetSecsItem(msg.Root, xPath);
                        pItem.Delete();
                    }
                }

                pItem = msg.Root;
                nItem = pItem.AddNew("1");
                nItem.Name = "L3";
                nItem.Description = "";
                nItem.Format = 0;
                nItem.NLB = 0;

                if (vfei.GetVarTriplet("CMD_TYPE").Value.ToString() == "GOAUTO")
                {
                    accessMode = 1;
                }
                else if (vfei.GetVarTriplet("CMD_TYPE").Value.ToString() == "GOMANUAL")
                {
                    accessMode = 0;
                }

                pItem = action.Parent.GetSecsItem(msg.Root, xPath);
                nItem = pItem.AddNew("1");
                nItem.Name = "PGRPACTION";
                nItem.Description = "Port Group Action";
                nItem.Format = SECS_FORMAT.ASCII;
                nItem.NLB = 0;
                nItem.Value = "ChangeAccess";

                pItem = action.Parent.GetSecsItem(msg.Root, xPath);
                nItem = pItem.AddNew("2");
                nItem.Name = "PORTGRPNAME";
                nItem.Description = "Port Group Name";
                nItem.Format = SECS_FORMAT.ASCII;
                nItem.NLB = 0;

                if (vfei.TryGetVar(ToolType.ITEM_PORT_ID, out temTriplet))
                {
                    nItem.Value = temTriplet.Value;
                }

                pItem = action.Parent.GetSecsItem(msg.Root, xPath);
                nItem = pItem.AddNew("3");
                nItem.Name = "L1";
                nItem.Description = "";
                nItem.Format = 0;
                nItem.NLB = 0;

                pItem = action.Parent.GetSecsItem(msg.Root, xPath2);
                nItem = pItem.AddNew("1");
                nItem.Name = "L2";
                nItem.Description = "";
                nItem.Format = 0;
                nItem.NLB = 0;

                pItem = action.Parent.GetSecsItem(msg.Root, xPath3);
                nItem = pItem.AddNew("1");
                nItem.Name = "PARANAME";
                nItem.Description = "Parameter Name";
                nItem.Format = SECS_FORMAT.ASCII;
                nItem.NLB = 0;
                nItem.Value = "AccessMode";

                pItem = action.Parent.GetSecsItem(msg.Root, xPath3);
                nItem = pItem.AddNew("2");
                nItem.Name = "PARAMVAL";
                nItem.Description = "Parameter Value";
                nItem.Format = SECS_FORMAT.U1;
                nItem.NLB = 0;
                nItem.Value = accessMode;

                result = secsTrans;
            }
            finally
            {
                // ------
                // Cleaning Up
                // ------
                secsTrans = null;
                msg = null;
                pItem = null;
                nItem = null;
            }

            return result;
        }
        public static SECSTransaction BuildS3F27(this AbstractDriverAction action, VfeiMessage vfei)
        {
            var secsTrans = action.Parent.NewTransaction("S3F27");
            secsTrans.Description = "Access Mode Change";
            var msg = secsTrans.Primary;
            msg.Root.Name = "S3F27";
            msg.Root.Description = "Access Mode Change";
            msg.Function = 27;
            msg.Stream = 3;
            msg.Root.ClearChildren();

            var pItem = msg.Root.AddNew();
            pItem.Name = "L2";
            pItem.Description = "";
            pItem.Format = SECS_FORMAT.LIST;
            pItem.NLB = 1;

            var cmdType = vfei.GetVarValue(ToolType.ITEM_CMD_TYPE);

            var nItem = pItem.AddNew();
            nItem.Name = "ACCESSMODE";
            nItem.Description = "Access Mode 0-Manual 1-Auto";
            nItem.Format = SECS_FORMAT.U1;
            nItem.NLB = 1;
            nItem.Value = cmdType.ToUpper() == "GOAUTO" ? 1 : 0;

            nItem = pItem.AddNew();
            nItem.Name = "Ln";
            nItem.Description = "Carrier Attribute List";
            nItem.Format = SECS_FORMAT.LIST;
            nItem.NLB = 1;

            var portId = vfei.GetVarValue(ToolType.ITEM_PORT_ID);

            foreach(var pid in portId.Split(','))
            {
                var item = nItem.AddNew();  //Carrier_Attribute(i)
                item.Name = "PORTID";
                item.Description = "Port ID";
                item.Format = SECS_FORMAT.U1;
                item.NLB = 1;
                item.Value = pid;
            }

            return secsTrans;
        }
        public static SECSTransaction BuildS3F27_AccessMode(this AbstractDriverAction action, VfeiMessage vfei)
        {
            int accessMode = 0;
            var secsTrans = action.Parent.NewTransaction("S3F27");

            var msg = secsTrans.Primary;
            msg.Root.Name = "HCS";
            msg.Root.Description = "Host command send";
            msg.Function = 27;
            msg.Stream = 3;
            msg.Root.ClearChildren();

            var pItem = msg.Root.AddNew();
            pItem.Name = "L2a";
            pItem.Description = "";
            pItem.Format = 0;
            pItem.NLB = 0;

            var cmdType = vfei.GetVarValue("CMD_TYPE", "GOMANUAL");
            if (cmdType  == "GOAUTO")
            {
                accessMode = 1;
            }
            else if (cmdType == "GOMANUAL")
            {
                accessMode = 0;
            }

            var nItem = pItem.AddNew();
            nItem.Name = "RCMD";
            nItem.Description = "Remote command code";
            nItem.Format = SECS_FORMAT.U1;
            nItem.NLB = 0;
            nItem.Value = accessMode;

            nItem = pItem.AddNew();
            nItem.Name = "Ln";
            nItem.Description = "";
            nItem.Format = 0;
            nItem.NLB = 0;

            pItem = nItem;
            nItem = pItem.AddNew();
            nItem.Name = "PTN";
            nItem.Description = "Load Port Number";
            nItem.Format = SECS_FORMAT.U1;
            nItem.NLB = 0;
            nItem.Value = Convert.ToInt32(vfei.GetVarValue(ToolType.ITEM_PORT_ID));
            return secsTrans;
        }
        public static SECSTransaction BuildS3F27_AccessMode_All(this AbstractDriverAction action, VfeiMessage vfei)
        {
            var secsTrans = action.Parent.NewTransaction("S3F27");

            var msg = secsTrans.Primary;
            msg.Root.Name = "S3F27";
            msg.Root.Description = "Access Mode Change";
            msg.Function = 27;
            msg.Stream = 3;
            msg.Root.ClearChildren();

            var pItem = msg.Root.AddNew();
            pItem.Name = "L2a";
            pItem.Description = "L2";
            pItem.Format = SECS_FORMAT.LIST;
            pItem.NLB = 1;

            var nItem = pItem.AddNew();
            nItem.Name = "ACCESSMODE";
            nItem.Description = "Access Mode 0-Manual 1-Auto";
            nItem.Format = SECS_FORMAT.U1;
            nItem.NLB = 1;
            nItem.Value = (vfei.GetVarTriplet(ToolType.ITEM_CMD_TYPE).Value.ToString() == "GOAUTO" ? 1 : 0);

            nItem = pItem.AddNew();
            nItem.Name = "Ln";
            nItem.Description = "Carrier Attribute List";
            nItem.Format = SECS_FORMAT.LIST;
            nItem.NLB = 1;

            // ==============================================================================================================================
            for (var count = (vfei.GetVarTriplet(ToolType.ITEM_PORT_ID).Value.ToString().Trim().Split(',')).GetLowerBound(0); count <= (vfei.GetVarTriplet(ToolType.ITEM_PORT_ID).Value.ToString().Trim().Split(',')).GetUpperBound(0); count++)
            {
                nItem = pItem.AddNew();     // Carrier_Attribute(i)
                nItem.Name = "PORTID";
                nItem.Description = "Port ID";
                nItem.Format = SECS_FORMAT.U1;
                nItem.NLB = 1;
                nItem.Value = Convert.ToInt32(vfei.GetVarTriplet(ToolType.ITEM_PORT_ID).Value.ToString().Trim().Split(',')[count]);
            }
            // ==============================================================================================================================
            return secsTrans;
        }
        public static SECSTransaction BuildS5F3_Enable_All_Alarm(this AbstractDriverAction action)
        {
            var secsTrans = action.Parent.NewTransaction(ToolType.S5F3);
            secsTrans.Primary.Root.Item(0).ClearChildren();

            var msg = secsTrans.Primary;
            msg.Root.Name = "EAS";
            msg.Root.Description = "Enable All Alarm Send";
            msg.Function = 3;
            msg.Stream = 5;

            var pItem = msg.Root;
            var nItem = pItem.Item(0);
            nItem.Name = "L2";
            nItem.Description = "";
            nItem.Format = SECS_FORMAT.LIST;
            nItem.NLB = 0;

            pItem = msg.Root.Item(0);
            pItem.AddNew("0");
            nItem = pItem.Item(0);
            nItem.Name = "ALED";
            nItem.Description = "Enable All Alarm";
            nItem.Format = SECS_FORMAT.BINARY;
            nItem.NLB = 0;
            nItem.Value = 128;

            pItem = msg.Root.Item(0);
            pItem.AddNew("1");
            nItem = pItem.Item(1);
            nItem.Name = "ALID";
            nItem.Description = "Alarm identification";
            nItem.Format = SECS_FORMAT.U4;
            nItem.NLB = 0;

            return secsTrans;
        }
        public static SECSTransaction BuildS6F23_Purge_Pool(this AbstractDriverAction action)
        {
            var secsTrans = action.Parent.NewTransaction(ToolType.S6F23);
            secsTrans.Primary.Root.ClearChildren();
            secsTrans.Primary.Root.AddNew("RSDC", SECS_FORMAT.U1, "RSDC").Value = "1";
            secsTrans.Primary.Root.Description = "Purge Spool";
            return secsTrans;
        }
        public static SECSTransaction BuildCarrierAction(this AbstractDriverAction action, VfeiMessage vfei)
        {
            var secsTrans = action.Parent.NewTransaction("S3F17");
            var msg = secsTrans.Primary;
            msg.Root.Name = "S3F17";
            msg.Root.Description = "Carrier Action Request";
            msg.Function = 17;
            msg.Stream = 3;
            msg.Root.ClearChildren();

            var pItem = msg.Root;
            var nItem = pItem.AddNew();
            nItem.Name = "L5";
            nItem.Description = "";
            nItem.Format = SECS_FORMAT.LIST;
            nItem.NLB = 0;

            pItem = msg.Root.Item(0);
            nItem = pItem.AddNew();
            nItem.Name = "DATAID";
            nItem.Description = "Data ID";
            nItem.Format = action.Parent.TypeOfDataID;
            nItem.NLB = 0;
            nItem.Value = 10;

            pItem = msg.Root.Item(0);
            nItem = pItem.AddNew();
            nItem.Name = "CARRIERACTION";
            nItem.Description = "Carrier Action";
            nItem.Format = SECS_FORMAT.ASCII;
            nItem.NLB = 0;

            nItem.Value = vfei.GetVarValue("CMD_TYPE");

            pItem = msg.Root.Item(0);
            nItem = pItem.AddNew();
            nItem.Name = "CARRIERSPEC";
            nItem.Description = "Carrier Object Specifier";
            nItem.Format = SECS_FORMAT.ASCII;
            nItem.NLB = 0;

            if (vfei.GetVarTriplet("CMD_TYPE").Value.ToString() == "CancelCarrierAtPort")
            {
                nItem.Value = "";
            }
            else
            {
                nItem.Value = vfei.GetVarValue("CARRIER_ID");
            }

            pItem = msg.Root.Item(0);
            nItem = pItem.AddNew();
            nItem.Name = "PTN";
            nItem.Description = "Material port number";
            nItem.Format = SECS_FORMAT.U1;
            nItem.NLB = 0;

            nItem.Value = vfei.GetVarValue("PORT_ID");

            pItem = msg.Root.Item(0);
            nItem = pItem.AddNew();
            nItem.Name = "Ln";
            nItem.Description = "";
            nItem.Format = SECS_FORMAT.LIST;
            nItem.NLB = 0;

            return secsTrans;
        }
        public static SECS_FORMAT FormatAscii2WinSecs(string secsFormat)
        {
            string procedureID = String.Empty;

            try
            {
                procedureID = "FormatAscii2WinSecs";

                switch (secsFormat.ToString().ToUpper())
                {
                    case "A":
                    case "ASCII":
                        return SECS_FORMAT.ASCII;
                    case "U1":
                        return SECS_FORMAT.U1;
                    case "U2":
                        return SECS_FORMAT.U2;
                    case "U4":
                        return SECS_FORMAT.U4;
                    case "U8":
                        return SECS_FORMAT.U8;
                    case "I1":
                        return SECS_FORMAT.I1;
                    case "I2":
                        return SECS_FORMAT.I2;
                    case "I4":
                        return SECS_FORMAT.I4;
                    case "I8":
                        return SECS_FORMAT.I8;
                    case "F4":
                        return SECS_FORMAT.F4;
                    case "F8":
                        return SECS_FORMAT.F8;
                    case "B":
                    case "BINARY":
                    case "BIN":
                    case "BYTE":
                        return SECS_FORMAT.BINARY;
                    case "BOOL":
                    case "BOOLEAN":
                        return SECS_FORMAT.BOOLEAN;
                    case "JIS":
                        return SECS_FORMAT.JIS8;
                    case "VAR":
                    case "V":
                        return SECS_FORMAT.CHAR2;  //Var
                    default:
                        return SECS_FORMAT.CHAR2;
                }
            }
            catch
            {
                return SECS_FORMAT.CHAR2;
            }
        }
    }
}
