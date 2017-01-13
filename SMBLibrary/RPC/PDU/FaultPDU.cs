/* Copyright (C) 2014-2017 Tal Aloni <tal.aloni.il@gmail.com>. All rights reserved.
 * 
 * You can redistribute this program and/or modify it under the terms of
 * the GNU Lesser Public License as published by the Free Software Foundation,
 * either version 3 of the License, or (at your option) any later version.
 */
using System;
using System.Collections.Generic;
using System.Text;
using Utilities;

namespace SMBLibrary.RPC
{
    /// <summary>
    /// rpcconn_fault_hdr_t
    /// </summary>
    public class FaultPDU : RPCPDU
    {
        public uint AllocationHint;
        public ushort ContextID;
        public byte CancelCount;
        public byte Reserved;
        public uint Status;
        public uint Reserved2;
        public byte[] Data;
        public byte[] AuthVerifier;

        public FaultPDU() : base()
        {
            PacketType = PacketTypeName.Fault;
            AuthVerifier = new byte[0];
        }

        public FaultPDU(byte[] buffer, int offset) : base(buffer, offset)
        {
            offset += RPCPDU.CommonFieldsLength;
            AllocationHint = LittleEndianReader.ReadUInt32(buffer, ref offset);
            ContextID = LittleEndianReader.ReadUInt16(buffer, ref offset);
            CancelCount = ByteReader.ReadByte(buffer, ref offset);
            Reserved = ByteReader.ReadByte(buffer, ref offset);
            Status = LittleEndianReader.ReadUInt32(buffer, ref offset);
            Reserved2 = LittleEndianReader.ReadUInt32(buffer, ref offset);
            int dataLength = FragmentLength - AuthLength - offset;
            Data = ByteReader.ReadBytes(buffer, ref offset, dataLength);
            AuthVerifier = ByteReader.ReadBytes(buffer, offset, AuthLength);
        }

        public override byte[] GetBytes()
        {
            AuthLength = (ushort)AuthVerifier.Length;
            FragmentLength = (ushort)(RPCPDU.CommonFieldsLength + 16 + Data.Length + AuthVerifier.Length);
            byte[] buffer = new byte[FragmentLength];
            WriteCommonFieldsBytes(buffer);
            int offset = RPCPDU.CommonFieldsLength;
            LittleEndianWriter.WriteUInt32(buffer, ref offset, AllocationHint);
            LittleEndianWriter.WriteUInt16(buffer, ref offset, ContextID);
            ByteWriter.WriteByte(buffer, ref offset, CancelCount);
            ByteWriter.WriteByte(buffer, ref offset, Reserved);
            LittleEndianWriter.WriteUInt32(buffer, ref offset, Status);
            LittleEndianWriter.WriteUInt32(buffer, ref offset, Reserved2);
            ByteWriter.WriteBytes(buffer, ref offset, Data);
            ByteWriter.WriteBytes(buffer, ref offset, AuthVerifier);
            return buffer;
        }
    }
}
