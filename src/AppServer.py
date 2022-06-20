from flask import Flask, request, jsonify, json
app = Flask(__name__)
import pandas as pd
disease_from_sym = pd.read_csv(r"sym_dis_prob_mat_final.csv")
sym_from_disease = pd.read_csv(r"sym_dis_prob_mat_v2.csv")


@app.route('/GetDiseaseFromSym', methods=['POST'])
def start():

    data = request.json
    syms = data.values()
    remove_list = []
    res = disease_from_sym[(disease_from_sym.code_sym.isin(syms)) & (~disease_from_sym.code_disease.isin(remove_list))].groupby(['code_disease','disease_desc'], as_index= False).sum().sort_values('score', ascending = False).reset_index(drop = True).head(5)
    response = res.to_dict()
    print(res)
    return jsonify(response)

@app.route('/GetSymFromDisease', methods=['POST'])
def start2():

    data = request.json
    disease = list(data.values())[0]
    remove_list = []
    res = sym_from_disease[(sym_from_disease.code_disease==disease)].groupby(['code_sym','symptom_desc'], as_index= False).sum().sort_values('prob', ascending = False).reset_index(drop = True).head(5)
    response = res.to_dict()
    print(res)
    return jsonify(response)

if __name__ == '__main__':
    app.run(host='0.0.0.0', port=105)
