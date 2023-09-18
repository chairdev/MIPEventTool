let fs = require("fs");
let num = process.argv[2];
let data = fs.readFileSync(`files/${num}.bin`);

let map = require("./charmap");

let base = 0x80100000;
data = data.subarray(8, data.readUint32LE(4) - base);

let labels = {};
let isValidPointer = (ptr) => {
  ptr -= base;
  return ptr >= 0 && ptr < data.byteLength;
};

for (let i = 0; i < data.byteLength - 4; i += 4) {
  let ptr = data.readUint32LE(i);
  if (isValidPointer(ptr)) {
    labels[ptr - base] = "D_" + ptr.toString(16);
  }
}
console.log(labels);
let strings = {};

for (let i = 0; i < data.byteLength - 8; i += 4) {
  let v = data.readUint32LE(i);
  let p = data.readUint32LE(i + 4);
  if (v == 0x55ff && isValidPointer(p)) {
    strings[p - base] = true;
  }
}

let findNextLabel = (p) => {
  while (p++ < data.length) {
    if (labels[p]) break;
  }
  return p;
};

let ptr = 0;
let lines = [];
let nextLabel = findNextLabel(0);
let dwEmitted = false;
while (ptr < data.length) {
  if (labels[ptr]) {
    lines.push(labels[ptr].toString() + ":");
    if (strings[ptr]) {
      let ended = false;
      let str = "";
      let lineData = [];
      while (ptr < data.byteLength && !ended) {
        let v = data[ptr++];
        if (v == 0x80) {
          v = data[ptr++];
        }

        if (map.from[v] !== undefined) {
          str += map.from[v];
        } else if (v == 0xff) {
          if (str.length) lineData.push(JSON.stringify(str));
          str = "";
          switch (data[ptr++]) {
            case 3:
              lineData.push("NL");
              lines.push(lineData.join(" "));
              lineData = [];
              break;
            case 5:
              lineData.push(`delay(${data[ptr++]})`);
              break;
            case 6:
              lineData.push(`color(${data[ptr++]})`);
              break;
            case 1:
              lineData.push(`end`);
              lines.push(lineData.join(" "));
              ended = true;
              break;
            case 2:
              lineData.push("wait");
              break;
            case 0x0e:
              lineData.push(`op_0e(${data[ptr++]})`);
              break;
            default:
              console.log((ptr + 8).toString(16));
              lineData.push(
                `op_${data[ptr - 1].toString(16).padStart(2, "0")}`
              );
          }
        } else {
          str += `\\x${v.toString(16).padStart(2, "0")}`;
        }
      }
      if (!ended) {
        throw new Error("Bad string");
      }
      continue;
    }
  }
  if (
    (ptr & 3) == 0 &&
    ptr < data.length - 4 &&
    isValidPointer(data.readUint32LE(ptr))
  ) {
    let v = data.readUint32LE(ptr);
    if (labels[v-base]) {
      lines.push(labels[v-base]);
      ptr += 4;
      continue;
    }
  }
  if (
    lines.length &&
    lines[lines.length - 1].length < 5 * 15 &&
    !lines[lines.length - 1].endsWith(":")
  ) {
    lines[lines.length - 1] += ` 0x${data[ptr].toString(16).padStart(2, "0")}`;
  } else {
    lines.push(`0x${data[ptr].toString(16).padStart(2, "0")}`);
  }
  ptr++;
  //   let next = findNextLabel(ptr);
  //   let diff = next - ptr;

  //   if (ptr & 3) {
  //     let bytes = [];
  //     while (ptr & 3) bytes.push(data[ptr++]);
  //   }
  //   if (diff > 4) {
  //     // lines.push(`.dw`)
  //   }
}

// let ptr = 0;
// let str = "";
// while (ptr < data.byteLength) {
//   let v = data[ptr++];
//   if (v == 0x80) {
//     v = data[ptr++];
//   }

//   if (map.from[v] !== undefined) {
//     str += map.from[v];
//   } else if (v == 0xff) {
//     switch (data[ptr]) {
//       case 3:
//         str += "\n";
//         break;
//       case 5:
//         ptr++;
//         str += `[delay(${data[ptr]})]`;
//         break;
//       case 6:
//         ptr++;
//         str += `[color(${data[ptr]})]`;
//         break;
//       case 1:
//         console.log((base + ptr + 1).toString(16));
//         str += "[end]\n";
//         ptr++;
//         while (data[ptr] == 0) ptr++;
//         ptr--;
//         break;
//       case 2:
//         str += "[wait]";
//         break;
//       case 0x0e:
//         ptr++;
//         str += `[0e(${data[ptr]})]`;
//         break;
//       default:
//         str += `[${data[ptr].toString(16).padStart(2, "0")}]`;
//     }
//     ptr++;
//   } else {
//     str += `\\${v.toString(16).padStart(2, "0")}`;
//   }
// }

fs.writeFileSync(`output_${num}.txt`, lines.join("\n"));
