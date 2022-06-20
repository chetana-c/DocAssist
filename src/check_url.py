import requests

url = 'http://127.0.0.1:105/GetDiseaseFromSym'

headers = {'Content-type': 'application/json'}
myobj = '{"sym1": "r0902", "sym2": "r0602"}'
x = requests.post(url, headers=headers, data = myobj)
print(x.json())


url = 'http://127.0.0.1:105/GetSymFromDisease'

headers = {'Content-type': 'application/json'}
myobj = '{"disease1": "j189"}'
x = requests.post(url, headers=headers, data = myobj)
print(x.json())
