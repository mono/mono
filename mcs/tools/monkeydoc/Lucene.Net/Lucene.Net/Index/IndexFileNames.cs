/* 
 * Licensed to the Apache Software Foundation (ASF) under one or more
 * contributor license agreements.  See the NOTICE file distributed with
 * this work for additional information regarding copyright ownership.
 * The ASF licenses this file to You under the Apache License, Version 2.0
 * (the "License"); you may not use this file except in compliance with
 * the License.  You may obtain a copy of the License at
 * 
 * http://www.apache.org/licenses/LICENSE-2.0
 * 
 * Unless required by applicable law or agreed to in writing, software
 * distributed under the License is distributed on an "AS IS" BASIS,
 * WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 * See the License for the specific language governing permissions and
 * limitations under the License.
 */

using System;

namespace Mono.Lucene.Net.Index
{
	
	/// <summary> Useful constants representing filenames and extensions used by lucene
	/// 
	/// </summary>
	/// <version>  $rcs = ' $Id: Exp $ ' ;
	/// </version>
	public sealed class IndexFileNames
	{
		
		/// <summary>Name of the index segment file </summary>
		public /*internal*/ const System.String SEGMENTS = "segments";
		
		/// <summary>Name of the generation reference file name </summary>
		public /*internal*/ const System.String SEGMENTS_GEN = "segments.gen";
		
		/// <summary>Name of the index deletable file (only used in
		/// pre-lockless indices) 
		/// </summary>
		public /*internal*/ const System.String DELETABLE = "deletable";
		
		/// <summary>Extension of norms file </summary>
		public /*internal*/ const System.String NORMS_EXTENSION = "nrm";
		
		/// <summary>Extension of freq postings file </summary>
		public /*internal*/ const System.String FREQ_EXTENSION = "frq";
		
		/// <summary>Extension of prox postings file </summary>
		public /*internal*/ const System.String PROX_EXTENSION = "prx";
		
		/// <summary>Extension of terms file </summary>
		public /*internal*/ const System.String TERMS_EXTENSION = "tis";
		
		/// <summary>Extension of terms index file </summary>
		public /*internal*/ const System.String TERMS_INDEX_EXTENSION = "tii";
		
		/// <summary>Extension of stored fields index file </summary>
		public /*internal*/ const System.String FIELDS_INDEX_EXTENSION = "fdx";
		
		/// <summary>Extension of stored fields file </summary>
		public /*internal*/ const System.String FIELDS_EXTENSION = "fdt";
		
		/// <summary>Extension of vectors fields file </summary>
		public /*internal*/ const System.String VECTORS_FIELDS_EXTENSION = "tvf";
		
		/// <summary>Extension of vectors documents file </summary>
		public /*internal*/ const System.String VECTORS_DOCUMENTS_EXTENSION = "tvd";
		
		/// <summary>Extension of vectors index file </summary>
		public /*internal*/ const System.String VECTORS_INDEX_EXTENSION = "tvx";
		
		/// <summary>Extension of compound file </summary>
		public /*internal*/ const System.String COMPOUND_FILE_EXTENSION = "cfs";
		
		/// <summary>Extension of compound file for doc store files</summary>
		public /*internal*/ const System.String COMPOUND_FILE_STORE_EXTENSION = "cfx";
		
		/// <summary>Extension of deletes </summary>
		internal const System.String DELETES_EXTENSION = "del";
		
		/// <summary>Extension of field infos </summary>
		public /*internal*/ const System.String FIELD_INFOS_EXTENSION = "fnm";
		
		/// <summary>Extension of plain norms </summary>
		public /*internal*/ const System.String PLAIN_NORMS_EXTENSION = "f";
		
		/// <summary>Extension of separate norms </summary>
		public /*internal*/ const System.String SEPARATE_NORMS_EXTENSION = "s";
		
		/// <summary>Extension of gen file </summary>
		public /*internal*/ const System.String GEN_EXTENSION = "gen";
		
		/// <summary> This array contains all filename extensions used by
		/// Lucene's index files, with two exceptions, namely the
		/// extension made up from <code>.f</code> + a number and
		/// from <code>.s</code> + a number.  Also note that
		/// Lucene's <code>segments_N</code> files do not have any
		/// filename extension.
		/// </summary>
		public /*internal*/ static readonly System.String[] INDEX_EXTENSIONS = new System.String[]{COMPOUND_FILE_EXTENSION, FIELD_INFOS_EXTENSION, FIELDS_INDEX_EXTENSION, FIELDS_EXTENSION, TERMS_INDEX_EXTENSION, TERMS_EXTENSION, FREQ_EXTENSION, PROX_EXTENSION, DELETES_EXTENSION, VECTORS_INDEX_EXTENSION, VECTORS_DOCUMENTS_EXTENSION, VECTORS_FIELDS_EXTENSION, GEN_EXTENSION, NORMS_EXTENSION, COMPOUND_FILE_STORE_EXTENSION};
		
		/// <summary>File extensions that are added to a compound file
		/// (same as above, minus "del", "gen", "cfs"). 
		/// </summary>
		public /*internal*/ static readonly System.String[] INDEX_EXTENSIONS_IN_COMPOUND_FILE = new System.String[]{FIELD_INFOS_EXTENSION, FIELDS_INDEX_EXTENSION, FIELDS_EXTENSION, TERMS_INDEX_EXTENSION, TERMS_EXTENSION, FREQ_EXTENSION, PROX_EXTENSION, VECTORS_INDEX_EXTENSION, VECTORS_DOCUMENTS_EXTENSION, VECTORS_FIELDS_EXTENSION, NORMS_EXTENSION};
		
		public /*internal*/ static readonly System.String[] STORE_INDEX_EXTENSIONS = new System.String[]{VECTORS_INDEX_EXTENSION, VECTORS_FIELDS_EXTENSION, VECTORS_DOCUMENTS_EXTENSION, FIELDS_INDEX_EXTENSION, FIELDS_EXTENSION};
		
		public /*internal*/ static readonly System.String[] NON_STORE_INDEX_EXTENSIONS = new System.String[]{FIELD_INFOS_EXTENSION, FREQ_EXTENSION, PROX_EXTENSION, TERMS_EXTENSION, TERMS_INDEX_EXTENSION, NORMS_EXTENSION};
		
		/// <summary>File extensions of old-style index files </summary>
		public /*internal*/ static readonly System.String[] COMPOUND_EXTENSIONS = new System.String[]{FIELD_INFOS_EXTENSION, FREQ_EXTENSION, PROX_EXTENSION, FIELDS_INDEX_EXTENSION, FIELDS_EXTENSION, TERMS_INDEX_EXTENSION, TERMS_EXTENSION};
		
		/// <summary>File extensions for term vector support </summary>
		public /*internal*/ static readonly System.String[] VECTOR_EXTENSIONS = new System.String[]{VECTORS_INDEX_EXTENSION, VECTORS_DOCUMENTS_EXTENSION, VECTORS_FIELDS_EXTENSION};
		
		/// <summary> Computes the full file name from base, extension and
		/// generation.  If the generation is -1, the file name is
		/// null.  If it's 0, the file name is 
		/// If it's > 0, the file name is 
		/// 
		/// </summary>
		/// <param name="base">-- main part of the file name
		/// </param>
		/// <param name="extension">-- extension of the filename (including .)
		/// </param>
		/// <param name="gen">-- generation
		/// </param>
		public /*internal*/ static System.String FileNameFromGeneration(System.String base_Renamed, System.String extension, long gen)
		{
			if (gen == SegmentInfo.NO)
			{
				return null;
			}
			else if (gen == SegmentInfo.WITHOUT_GEN)
			{
				return base_Renamed + extension;
			}
			else
			{
#if !PRE_LUCENE_NET_2_0_0_COMPATIBLE
				return base_Renamed + "_" + SupportClass.Number.ToString(gen) + extension;
#else
				return base_Renamed + "_" + System.Convert.ToString(gen, 16) + extension;
#endif
			}
		}
		
		/// <summary> Returns true if the provided filename is one of the doc
		/// store files (ends with an extension in
		/// STORE_INDEX_EXTENSIONS).
		/// </summary>
		internal static bool IsDocStoreFile(System.String fileName)
		{
			if (fileName.EndsWith(COMPOUND_FILE_STORE_EXTENSION))
				return true;
			for (int i = 0; i < STORE_INDEX_EXTENSIONS.Length; i++)
				if (fileName.EndsWith(STORE_INDEX_EXTENSIONS[i]))
					return true;
			return false;
		}
		
		internal static System.String SegmentFileName(System.String segmentName, System.String ext)
		{
			return segmentName + "." + ext;
		}
	}
}
