let fs = require("fs");

let ptr = 0;
let file = fs.readFileSync("E0.BIN");

let files = [];
while (true) {
  let sector = file.readUint16LE(ptr);
  if (sector == 0) break;
  files.push(sector);
  ptr += 2;
}
fs.mkdirSync("files", { recursive: true });
let counts = [];
for (let i = 0; i < files.length - 1; i++) {
  let start = files[i];
  let end = files[i + 1];
  let data = file.subarray(start * 0x800, end * 0x800);
  let c = [];
  for (let i = 0; i < 26; i++) {
    c.push(data.readInt16LE(8 + i * 2));
  }
  counts.push(c);
  fs.writeFileSync(`files/${i}.bin`, data);
}
fs.writeFileSync(`counts.csv`, counts.map((a) => a.join(",")).join("\n"));
