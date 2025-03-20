{ pkgs ? import <nixpkgs> {} }:

pkgs.mkShell{
  shellHook = ''
    export SHELL=$(which zsh)
    nvim && exit
  '';
}
