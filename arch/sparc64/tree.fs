include config.fs

\ -------------------------------------------------------------------------
\ UPA encode/decode unit
\ -------------------------------------------------------------------------

\ The sabre reg bustype is 0x1fe: UPA node-id 0b111 in bits[8:6] plus the
\ 5-bit port-id in bits[5:1] (bit 0 zero). Solaris names root children by
\ upa-portid, so map between bustype and portid here.

: upa-portid>reg  ( portid -- bustype )
  dup if 1 lshift 1c0 or then
;

\ decode-unit: parse "<portid>,<offset>" and return reg cells. find-child
\ stores the TOP stack value into unit_phys[0] (reg cell 0), so the bustype
\ must be on top and the offset below it.
: decode-unit-upa ( str len -- offset bustype )
  2 parse-nhex
  upa-portid>reg
;

\ encode-unit: render reg cells as "<portid>,<offset>". decode-phys pushes
\ the offset (low cell) on the bottom and the bustype on top.
: encode-unit-upa ( offset bustype -- str len )
  swap
  pocket tohexstr
  " ," pocket tmpstrcat >r
  rot
  1 rshift 1f and
  pocket tohexstr
  r> tmpstrcat drop
;

\ ---------
\ DMA words
\ ---------

: sparc64-dma-free  ( virt size -- )
  2drop
;

: sparc64-dma-map-in  ( virt size cacheable? -- devaddr )
  2drop
;

: sparc64-dma-map-out  ( virt devaddr size -- )
  (dma-sync)
;

['] sparc64-dma-free to (dma-free)
['] sparc64-dma-map-in to (dma-map-in)
['] sparc64-dma-map-out to (dma-map-out)

\ -------------------------------------------------------------
\ device-tree
\ -------------------------------------------------------------

" /" find-device
  2 encode-int " #address-cells" property
  2 encode-int " #size-cells" property
  " sun4u" encode-string " compatible" property

  : encode-unit encode-unit-upa ;
  : decode-unit decode-unit-upa ;

  : dma-sync  ( virt devaddr size -- )
    (dma-sync)
  ;

  : dma-alloc  ( size -- virt )
    (dma-alloc)
  ;

  : dma-free  ( virt size -- )
    (dma-free)
  ;

  : dma-map-in  ( virt size cacheable? -- devaddr )
    (dma-map-in)
  ;

  : dma-map-out  ( virt devaddr size -- )
    (dma-map-out)
  ;

new-device
  " memory" device-name
  " memory" device-type
  external
  : open true ;
  : close ;
  \ see arch/sparc64/lib.c for methods
finish-device

new-device
  " virtual-memory" device-name
  external
  \ see arch/sparc64/lib.c for methods
finish-device

" /options" find-device
  " disk" encode-string " boot-from" property

" /openprom" find-device
  " OBP 3.10.24 1999/01/01 01:01" encode-string " version" property
