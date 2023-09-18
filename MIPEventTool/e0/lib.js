let fs = require("fs");
// let num = process.argv[2];
// let data = fs.readFileSync(`files/${num}.bin`);

let map = require("./charmap");

// let base = 0x80100000;
// let end = base + data.byteLength;
// data = data.subarray(8);
// let isValidPointer = (ptr) => {
//   ptr -= 0x80100000;
//   return ptr >= 0 && ptr < data.byteLength;
// };

// let counts = [];
// for (let i = 0; i < 26; i++) {
//   let v = data.readInt16LE(i * 2);
//   //   if (v == 0xffff) counts.push(0);
//   counts.push(v);
// }
// {
//   let count = 0;
//   // let ptr = 26*2;
//   for (let i = 0; i < data.byteLength - 8; i += 2) {
//     let v = data.readUint16LE(i);
//     if (v == 0x55ff && data.readUint16LE(i + 2) == 0) {
//       let p = data.readUint32LE(i + 4) - base;
//       if (p > 0 && p < data.byteLength) {
//         console.log(i.toString(16));
//         count++;
//       }
//     }
//   }
//   console.log(count);
// }
// console.log(counts);

module.exports.parse = (ptr, data) => {
  let str = "";
  while (ptr < data.byteLength) {
    let v = data[ptr++];
    if (v == 0x80) {
      v = data[ptr++];
    }

    if (map.from[v] !== undefined) {
      str += map.from[v];
    } else if (v == 0xff) {
      switch (data[ptr]) {
        case 3:
          str += "\n";
          break;
        case 5:
          ptr++;
          str += `[delay(${data[ptr]})]`;
          break;
        case 6:
          ptr++;
          str += `[color(${data[ptr]})]`;
          break;
        case 1:
        //   console.log((base + ptr + 1).toString(16));
          str += "[end]";
          ptr++;
          while (data[ptr] == 0) ptr++;
          ptr--;
          return [str, ptr + 1];
          break;
        case 2:
          str += "[wait]";
          break;
        case 0x0e:
          ptr++;
          str += `[0e(${data[ptr]})]`;
          break;
        default:
          str += `[${data[ptr].toString(16).padStart(2, "0")}]`;
      }
      ptr++;
    } else {
      str += `\\${v.toString(16).padStart(2, "0")}`;
    }
  }
  throw new Error("Bad string");
};
