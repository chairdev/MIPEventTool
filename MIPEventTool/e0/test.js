let fs = require("fs");
const { parse } = require("./lib");
let files = fs.readdirSync("files");
for (const filename of files) {
  let data = fs.readFileSync("files/" + filename);
  data = data.subarray(8, data.readUint32LE(4) - 0x80100000);
  let counts = [];
  for (let i = 0; i < 26; i++) {
    let v = data.readInt16LE(i * 2);
    counts.push(v);
  }
  let isValidPointer = (ptr) => {
    ptr -= 0x80100000;
    return ptr >= 0 && ptr < data.byteLength - 4;
  };
  let funcCount = 0;
  //   let checkIsFunction = (ptr) => {
  //     // console.log(ptr.toString(16));
  //     if (ptr == 0xffffffff) return true;

  //     let res =
  //       isValidPointer(ptr) && data.readUint32LE(ptr - 0x80100000) == 0x20ff;
  //     if (res) funcCount++;
  //     return res;
  //   };
  //   // for (let i = 0; i < data.byteLength - 4; i += 4) {
  //   //   let p = data.readUint32LE(i);
  //   //   if (p >= base && p <= end) {
  //   //     console.log(p.toString(16), i.toString(16));
  //   //   }
  //   // }

  //   if (!checkIsFunction(data.readUint32LE(0x60))) {
  //     console.log("No");
  //     continue;
  //   }
  //   for (let i = 0; i < 64; i++) {
  //     let off = 0x1f4 + 36 * i;
  //     if (!checkIsFunction(data.readUint32LE(off))) {
  //       console.log("No " + filename + off.toString(16));
  //       return;
  //     }
  //   }
  //   console.log(filename);

  let min = 0xffffffff;
  let num = parseInt(filename);
  if(num >=10) continue;
  let ranges = [];
  let strings = {};
  for (let i = 0; i < data.byteLength - 8; i++) {
    let v = data.readUint32LE(i);
    // if (filename == "0.bin") 
    {
      if (v == 0x55ff) {
        let p = data.readUint32LE(i + 4);
        if (strings[p]) continue;
        let res = parse(p - 0x80100000, data);
        ranges.push([p - 0x80100000, res[1]]);
        strings[p] = res[0];
      }
    }
  }
  ranges.sort((a, b) => a[0] - b[0]);
  for (let i = 0; i < ranges.length - 1; i++) {
    if (ranges[i][1] != ranges[i + 1][0]) {
      console.log(
        "Uh oh ",filename,
        ranges[i].map((a) => a.toString(16)),
        ranges[i + 1].map((a) => a.toString(16))
      );
      let t = ranges[i][1];
      let s;
      while(t < ranges[i+1][0]) {
        [s, t] = parse(t, data);
        console.log(t.toString(16), s);
      }
      console.log(parse(ranges[i][1], data))
      console.log(parse(ranges[i][0], data))
    }
  }
  //   console.log(funcCount);
  //   console.log(strings);
}
