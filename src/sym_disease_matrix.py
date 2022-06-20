import pandas as pd
pd.set_option('display.max_columns', None)
pd.set_option('display.max_rows', None)
import pyspark.sql.functions as F
from biobert_embedding.embedding import BiobertEmbedding
import scipy


ccsr_map=spark.read.csv('/mnt/azureblobshare/hackathon/DXCCSR_v2021-2.csv', header=True, quote='"')
diseases = ['INF', 'NEO', 'BLD', 'END', 'MBD', 'NVS', 'EYE', 'EAR', 'CIR', 'RSP', 'DIG', 'SKN', 'MUS', 'GEN',
                     'PRG', 'PNL', 'MAL', 'INJ']
for icd_dx_col in ['ICD-10-CM CODE']:
    ccsr_map=ccsr_map.withColumn(icd_dx_col, F.lower(F.regexp_replace(icd_dx_col, '\.', '')))
icd_to_css=ccsr_map.select('ICD-10-CM CODE','ICD-10-CM CODE DESCRIPTION','Default CCSR CATEGORY OP','Default CCSR CATEGORY DESCRIPTION OP')\
                     .withColumnRenamed('ICD-10-CM CODE','ICD_10_CM_CODE')\
                     .withColumnRenamed('Default CCSR CATEGORY OP','Default_CCSR_CATEGORY_OP')\
                     .withColumnRenamed('Default CCSR CATEGORY DESCRIPTION OP','Default_CCSR_CATEGORY_DESCRIPTION_OP')\
                     .withColumn('CCSR_BODY_SYSTEMS', F.col('Default_CCSR_CATEGORY_OP').substr(1, 3))\
                     .select('ICD_10_CM_CODE','CCSR_BODY_SYSTEMS')
icd_descp = ccsr_map.select('ICD-10-CM CODE','ICD-10-CM CODE DESCRIPTION')\
                    .withColumnRenamed('ICD-10-CM CODE DESCRIPTION','code_description')\
                    .withColumnRenamed('ICD-10-CM CODE','code')\
                    .drop_duplicates(['code'])


def sym_disease_probability_matrix(code_table, start_date, end_date):
    diseases_cssr_body_sys = ['INF', 'NEO', 'BLD', 'END', 'MBD', 'NVS', 'EYE', 'EAR', 'CIR', 'RSP', 'DIG', 'SKN', 'MUS', 'GEN',
                'PRG', 'PNL', 'MAL', 'INJ']
    code_table = code_table.filter('date>= "{start_date}" and date<"{end_date}"'.format(**locals()))\
                           .drop_duplicates(['clm_head_id', 'code'])\
                           .join(icd_to_css, [code_table.code == icd_to_css.ICD_10_CM_CODE], 'left')
    symtoms = code_table.filter(F.col('CCSR_BODY_SYSTEMS')== 'SYM')\
                        .withColumnRenamed('code','code_sym')
    diseases = code_table.filter(F.col('CCSR_BODY_SYSTEMS').isin(diseases_cssr_body_sys)) \
                         .withColumnRenamed('code', 'code_disease')
    diseases_and_sym = diseases.join(symtoms,['clm_head_id'],'inner')
    sym_occur_count = diseases_and_sym.drop_duplicates(['clm_head_id', 'code_sym']) \
                                          .groupby('code_sym').agg(F.count('code_sym').alias('sym_occur_count'))

    disease_and_sym_count = diseases_and_sym.groupby('code_disease','code_sym').agg(F.count('code_disease').alias('disease_sym_occur_count'))\
                                             .join(sym_occur_count, ['code_sym'],'left')\
                                             .withColumn('prob', F.col('disease_sym_occur_count')/F.col('sym_occur_count'))
    return disease_and_sym_count


def disease_sym_probability_matrix(code_table, start_date, end_date):
    diseases_cssr_body_sys = ['INF', 'NEO', 'BLD', 'END', 'MBD', 'NVS', 'EYE', 'EAR', 'CIR', 'RSP', 'DIG', 'SKN', 'MUS', 'GEN',
                'PRG', 'PNL', 'MAL', 'INJ']
    code_table = code_table.filter('date>= "{start_date}" and date<"{end_date}"'.format(**locals()))\
                           .drop_duplicates(['clm_head_id', 'code'])\
                           .join(icd_to_css, [code_table.code == icd_to_css.ICD_10_CM_CODE], 'left')
    symtoms = code_table.filter(F.col('CCSR_BODY_SYSTEMS')== 'SYM')\
                        .withColumnRenamed('code','code_sym')
    diseases = code_table.filter(F.col('CCSR_BODY_SYSTEMS').isin(diseases_cssr_body_sys)) \
                         .withColumnRenamed('code', 'code_disease')
    diseases_and_sym = diseases.join(symtoms,['clm_head_id'],'inner')
    sym_occur_count = diseases_and_sym.drop_duplicates(['clm_head_id', 'code_disease']) \
                                          .groupby('code_disease').agg(F.count('code_disease').alias('disease_occur_count'))

    disease_and_sym_count = diseases_and_sym.groupby('code_disease','code_sym').agg(F.count('code_disease').alias('disease_sym_occur_count'))\
                                             .join(sym_occur_count, ['code_sym'],'left')\
                                             .withColumn('prob', F.col('disease_sym_occur_count')/F.col('disease_occur_count'))
    return disease_and_sym_count


code_table = spark.read.parquet('/mnt/subscriptionshare/snowflake/dev/data/processed_eods_monthly_cdf/202107/code_table')
sym_dis_prob_mat = sym_disease_probability_matrix(code_table, '2018-01-01', '2021-07-01')
sym_dis_prob_mat=sym_dis_prob_mat.join(icd_descp, [sym_dis_prob_mat.code_disease == icd_descp.code],'left')\
                                 .drop('code')\
                                 .withColumnRenamed('code_description','disease_desc')\
                                 .join(icd_descp, [sym_dis_prob_mat.code_sym == icd_descp.code],'left')\
                                 .drop('code')\
                                 .withColumnRenamed('code_description','symptom_desc')\
                                 .sort(F.col('code_sym').asc(),F.col('prob').desc())

sym_dis_prob_mat_df = sym_dis_prob_mat.toPandas()
sym_dis_prob_mat_df.to_csv('/mnt/azureblobshare/sym_dis_prob_mat_final.csv',index= False)


def get_embedding(text, type):
    biobert = BiobertEmbedding()
    if type == 'sentence':
        sentence_embedding = biobert.sentence_vector(text)
        return sentence_embedding
    elif type == 'word':
        word_embeddings = biobert.word_vector(text)
        return word_embeddings
    else:
        print("Type information not explicit")


def get_cosine_similarity(u,v):
    return 1 - scipy.spatial.distance.cosine(u, v)






