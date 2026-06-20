{pkgs}: {
  deps = [
    pkgs.libxkbcommon
    pkgs.xorg.libSM
    pkgs.xorg.libICE
    pkgs.xorg.libXrender
    pkgs.xorg.libXext
    pkgs.xorg.libX11
    pkgs.libGL
    pkgs.fontconfig
  ];
}
