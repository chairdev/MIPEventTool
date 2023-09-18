let lowercase = 0x31;
let uppercase = 0xa6;
let numbers = 0xc0;

let mapTo = {};
let mapFrom = {
  0x00: " ",
  0x61: "'",
  0x62: "{",
  0x63: "}",
  0x65: ",",
  0x68: ":",
  0x69: ";",
  0x6b: "=",
  0x6c: ">",
  0x6e: "\\[",
  0x6f: "]",
  0xa5: ".",
  0xd0: "?",
  0xd1: "!",
  0x63: "<",
};
for (let i = 0; i < 26; i++) {
  mapFrom[lowercase + i] = String.fromCharCode("a".charCodeAt(0) + i);
  mapFrom[uppercase + i] = String.fromCharCode("A".charCodeAt(0) + i);
  mapTo[String.fromCharCode("a".charCodeAt(0) + i)] = lowercase + i;
  mapTo[String.fromCharCode("A".charCodeAt(0) + i)] = uppercase + i;
  if (i < 10) {
    mapFrom[numbers + i] = String.fromCharCode("0".charCodeAt(0) + i);
    mapTo[String.fromCharCode("0".charCodeAt(0) + i)] = numbers + i;
  }
}

module.exports = {
  from: mapFrom,
  to: mapTo,
};
