# OpenBIOS — Solaris 10 sun4u boot support

This fork adds the changes needed for REAL FULL Sun Microsystems Solaris 10 (SPARC, `sun4u`) to boot
under QEMU on top of this OpenBIOS tree.

## What changed

OpenBIOS is the IEEE 1275 Open Firmware implementation QEMU uses to
publish the device tree that the Solaris kernel consumes. The stock tree
omits or mis-shapes several nodes and client-interface behaviours that
Solaris's drivers require, so the kernel aborts with
"Cannot find /devices" and "Cannot assemble drivers for root".

The commit in this fork:

- derives the PROM physical base from the reset PC, making the image
  relocatable to wherever QEMU loads it;
- names sabre root children by UPA port-id (`upa-portid>reg`) instead of
  the raw `id,lun` form;
- publishes ATA disks as direct children of the IDE controller
  (`dad@0,0`, `sd@...`) with SCSI-style `<target>,<lun>` unit addresses;
- identity-maps PCI-PCI bridge windows into the `ranges` property;
- fixes the e1000 `compatible` string, adds `lsi53c895`/`mpt` device
  entries, and adds a generic PCI-PCI bridge fallback;
- omits the ISA `fdthree` node, whose cheerio mapping makes Solaris's fd
  driver panic;
- guards the client-interface return handling so empty or unhandled
  results do not corrupt caller registers.

## Status

With this tree and the matching QEMU tree, the machine boots and reaches
the Solaris installer, and the root filesystem mounts.

The resulting system is **not stable**: it runs briefly and then
experiences a spurious crash, typically within an hour of boot. The crash
has not been root-caused.

## You need both repositories

This OpenBIOS fork is one half of the change. It must be paired with the
matching QEMU fork ([`paulsheer/qemu`](https://github.com/paulsheer/qemu)),
which fixes the cmd646 interrupt latch and the LSI 53C895 device identity.
Using only one of the two will not produce a bootable system.

Check out this tree as the `roms/openbios` submodule of that QEMU tree (see
its README for build instructions).
