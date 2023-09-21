import os
import struct
import shutil
import sys

def clean(st,nu):
    return(('0'*(nu-len(str(st))))+str(st))

def endstrip(inp, str):
    for i in range(0, len(inp), len(str)):
        if inp[::-1][i:i+len(str)] != str[::-1]:
            break
    if i!=0:
        return inp[:-1 * i]
    else: return inp

def phelp():
    print('''Persona 1 Bin Tool
oliviayellowcat#0530

p1bintool unpack [input bin] [output folder]
p1bintool pack [input folder] [output bin]
p1bintool btlpfix [original btlp.bin] [output btlp.bin] [original bin] [edited bin]''')

def unpack(filename, foldername):
    file = open(filename,'rb')
    indexes = []
    for i in range(0,1024):
        index = struct.unpack('h',file.read(2))[0] * 2048
        if index == 0:
            break
        indexes.append(index)

    if not os.path.isdir(foldername): os.mkdir(foldername)

    for i in range(0,len(indexes) - 1):
        sindex = clean(hex(i*2)[2:],3)
        
        file.seek(indexes[i])
        data = file.read(indexes[i+1]-indexes[i])
        
        newfile = open(foldername+'/'+sindex+'.bin', 'wb')
        newfile.write(data)
        newfile.close()

def pack(foldername, filename):
    allfiles = os.listdir(foldername)
    newfile = open(filename, 'wb')
    newfile.write(b'\x00' * 2048)
    pointer = 2048
    for i in allfiles:
        file = open(foldername+'/'+i, 'rb')
        data = file.read()
        file.close()
        index = int(i[:i.rfind('.')], 16)
        newfile.seek(index)
        newfile.write(struct.pack('h', pointer // 2048))
        newfile.seek(pointer)
        newfile.write(data)
        pointer += len(data)
        if (pointer % 2048)!=0:
            newfile.write(b'\x00' * (-pointer % 2048))
            pointer += (-pointer % 2048)
    newfile.seek(index + 2)
    newfile.write(struct.pack('h', pointer // 2048))
 
def btlpfix(orgbtlp, newbtlp, org, new):
    orgbtlpfile = open(orgbtlp, 'rb')
    orgbtlpdata = orgbtlpfile.read()
    newbtlpfile = open(newbtlp, 'wb')
    orgfile = open(org, 'rb')
    newfile = open(new, 'rb')
    orgheader = endstrip(orgfile.read(256), b'\x00')
    newheader = endstrip(newfile.read(256), b'\x00')
    print(newheader)
    print(orgheader)
    newbtlpdata = orgbtlpdata.replace(orgheader, newheader)
    newbtlpfile.write(newbtlpdata)
    
    orgfile.close()
    newfile.close()
    orgbtlpfile.close()
    newbtlpfile.close()
    
    

def main():
    args = [i.lower() for i in sys.argv[1:]]
    if len(args) < 1:
        phelp()
    else:
        if args[0] == 'unpack':
            if len(args) != 3: phelp()
            unpack(args[1], args[2])
        elif args[0] == 'pack':
            if len(args) != 3: phelp()
            pack(args[1], args[2])
        elif args[0] == 'btlpfix':
            if len(args) != 5: phelp()
            btlpfix(args[1], args[2], args[3], args[4])
        else:
            phelp()
    
main()