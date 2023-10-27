# Two-Level Tree Structure for Fast Pointer Lookup

The Boehm-Demers-Weiser conservative Garbage Collector uses a 2-level tree
data structure to aid in fast pointer identification. This data structure
is described in a bit more detail here, since

  1. Variations of the data structure are more generally useful.
  2. It appears to be hard to understand by reading the code.
  3. Some other collectors appear to use inferior data structures to solve the
  same problem.
  4. It is central to fast collector operation.  A candidate pointer
  is divided into three sections, the _high_, _middle_, and _low_ bits. The
  exact division between these three groups of bits is dependent on the
  detailed collector configuration.

The high and middle bits are used to look up an entry in the table described
here. The resulting table entry consists of either a block descriptor
(`struct hblkhdr *` or `hdr *`) identifying the layout of objects in the
block, or an indication that this address range corresponds to the middle of
a large block, together with a hint for locating the actual block descriptor.
Such a hint consist of a displacement that can be subtracted from the middle
bits of the candidate pointer without leaving the object.

In either case, the block descriptor (`struct hblkhdr`) refers to a table
of object starting addresses (the `hb_map` field). The starting address table
is indexed by the low bits if the candidate pointer. The resulting entry
contains a displacement to the beginning of the object, or an indication that
this cannot be a valid object pointer. (If all interior pointer are
recognized, pointers into large objects are handled specially,
as appropriate.)

## The Tree

The rest of this discussion focuses on the two level data structure used
to map the high and middle bits to the block descriptor.

The high bits are used as an index into the `GC_top_index` (really
`GC_arrays._top_index`) array. Each entry points to a `bottom_index` data
structure. This structure in turn consists mostly of an array `index` indexed
by the middle bits of the candidate pointer. The `index` array contains the
actual `hdr` pointers.

Thus a pointer lookup consists primarily of a handful of memory references,
and can be quite fast:

  1. The appropriate `bottom_index` pointer is looked up in `GC_top_index`,
  based on the high bits of the candidate pointer.
  2. The appropriate `hdr` pointer is looked up in the `bottom_index`
  structure, based on the middle bits.
  3. The block layout map pointer is retrieved from the `hdr` structure. (This
  memory reference is necessary since we try to share block layout maps.)
  4. The displacement to the beginning of the object is retrieved from the
  above map.

In order to conserve space, not all `GC_top_index` entries in fact point
to distinct `bottom_index` structures. If no address with the corresponding
high bits is part of the heap, then the entry points to `GC_all_nils`,
a single `bottom_index` structure consisting only of `NULL` `hdr` pointers.

`Bottom_index` structures contain slightly more information than just `hdr`
pointers. The `asc_link` field is used to link all `bottom_index` structures
in ascending order for fast traversal. This list is pointed to be
`GC_all_bottom_indices`. It is maintained with the aid of `key` field that
contains the high bits corresponding to the `bottom_index`.

## 64-bit addresses

In the case of 64-bit addresses, this picture is complicated slightly by the
fact that one of the index structures would have to be huge to cover the
entire address space with a two level tree. We deal with this by turning
`GC_top_index` into a chained hash table, instead of a simple array. This adds
a `hash_link` field to the `bottom_index` structure.

The _hash function_ consists of dropping the high bits. This is cheap
to compute, and guarantees that there will be no collisions if the heap
is contiguous and not excessively large.

## A picture

The following is an _ASCII_ diagram of the data structure used by GC_base (as
of GC v3.7, Apr 21, 1994). This was contributed by Dave Barrett.


        63                  LOG_TOP_SZ[11]  LOG_BOTTOM_SZ[10]   LOG_HBLKSIZE[13]
       +------------------+----------------+------------------+------------------+
     p:|                  |   TL_HASH(hi)  |                  |   HBLKDISPL(p)   |
       +------------------+----------------+------------------+------------------+
        \-----------------------HBLKPTR(p)-------------------/
        \------------hi-------------------/
                          \______ ________/ \________ _______/ \________ _______/
                                 V                   V                  V
                                 |                   |                  |
               GC_top_index[]    |                   |                  |
     ---      +--------------+   |                   |                  |
      ^       |              |   |                   |                  |
      |       |              |   |                   |                  |
     TOP      +--------------+<--+                   |                  |
     _SZ   +-<|      []      | *                     |                  |
    (items)|  +--------------+  if 0 < bi< HBLKSIZE  |                  |
      |    |  |              | then large object     |                  |
      |    |  |              | starts at the bi'th   |                  |
      v    |  |              | HBLK before p.        |             i    |
     ---   |  +--------------+                       |          (word-  |
           v                                         |         aligned) |
       bi= |GET_BI(p){->hash_link}->key==hi          |                  |
           v                                         |                  |
           |   (bottom_index)  \ scratch_alloc'd     |                  |
           |   ( struct  bi )  / by get_index()      |                  |
     ---   +->+--------------+                       |                  |
      ^       |              |                       |                  |
      ^       |              |                       |                  |
     BOTTOM   |              |   ha=GET_HDR_ADDR(p)  |                  |
    _SZ(items)+--------------+<----------------------+          +-------+
      |   +--<|   index[]    |                                  |
      |   |   +--------------+                      GC_obj_map: v
      |   |   |              |              from      / +-+-+-----+-+-+-+-+  ---
      v   |   |              |              GC_add   < 0| | |     | | | | |   ^
     ---  |   +--------------+             _map_entry \ +-+-+-----+-+-+-+-+   |
          |   |   asc_link   |                          +-+-+-----+-+-+-+-+ MAXOBJSZ
          |   +--------------+                      +-->| | |  j  | | | | |  +1
          |   |     key      |                      |   +-+-+-----+-+-+-+-+   |
          |   +--------------+                      |   +-+-+-----+-+-+-+-+   |
          |   |  hash_link   |                      |   | | |     | | | | |   v
          |   +--------------+                      |   +-+-+-----+-+-+-+-+  ---
          |                                         |   |<--MAX_OFFSET--->|
          |                                         |         (bytes)
    HDR(p)| GC_find_header(p)                       |   |<--MAP_ENTRIES-->|
          |                           \ from        |    =HBLKSIZE/WORDSZ
          |    (hdr) (struct hblkhdr) / alloc_hdr() |    (1024 on Alpha)
          +-->+----------------------+              |    (8/16 bits each)
    GET_HDR(p)| word   hb_sz (words) |              |
              +----------------------+              |
              | struct hblk *hb_next |              |
              +----------------------+              |
              |mark_proc hb_mark_proc|              |
              +----------------------+              |
              | char * hb_map        |>-------------+
              +----------------------+
              | ushort hb_obj_kind   |
              +----------------------+
              |   hb_last_reclaimed  |
     ---      +----------------------+
      ^       |                      |
     MARK_BITS|       hb_marks[]     |  * if hdr is free, hb_sz is the size of
    _SZ(words)|                      |  a heap chunk (struct hblk) of at least
      v       |                      |  MININCR*HBLKSIZE bytes (below),
     ---      +----------------------+  otherwise, size of each object in chunk.


Dynamic data structures above are interleaved throughout the heap in blocks
of size `MININCR * HBLKSIZE` bytes as done by `gc_scratch_alloc` which cannot
be freed; free lists are used (e.g. `alloc_hdr`). `HBLK`'s below are
collected.


                  (struct hblk)                                  HDR_BYTES
     ---      +----------------------+ < HBLKSIZE  ---            (bytes)
      ^       +-----hb_body----------+ (and WORDSZ) ^         ---   ---
      |       |                      |   aligned    |          ^     ^
      |       |                      |              |        hb_sz   |
      |       |                      |              |       (words)  |
      |       |      Object 0        |              |          |     |
      |       |                      |            i |(word-    v     |
      |       + - - - - - - - - - - -+ ---   (bytes)|aligned) ---    |
      |       |                      |  ^           |          ^     |
      |       |                      |  j (words)   |          |     |
      n *     |      Object 1        |  v           v        hb_sz BODY_SZ
     HBLKSIZE |                      |---------------          |   (words)
     (bytes)  |                      |                         v   MAX_OFFSET
      |       + - - - - - - - - - - -+                        ---  (bytes)
      |       |                      | !ALL_INTERIOR_POINTERS  ^     |
      |       |                      | sets j only for       hb_sz   |
      |       |      Object N        | valid object offsets.   |     |
      v       |                      | All objects WORDSZ      v     v
     ---      +----------------------+ aligned.               ---   ---
