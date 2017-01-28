namespace TpfModManager

module Types =
    type Author = {name: string; tpfNetId: int option}
    type Folder = Folder of string