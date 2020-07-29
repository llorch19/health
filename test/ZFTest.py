"""
Author: lwxiao
Date  : 2020-07-27
Description: 自动测试接口有效性
Comments
"""

import urllib3 as ul
import urllib.parse as up
import json
import random
import os

true = True
false = False
null = None
data0 = [1]
host = "http://192.168.21.24:25000"
http = ul.PoolManager()

f_log_names = list(os.walk("./TestLogs"))[0][2]
f_log_name = "test_log{0}.txt"
log_name_i = 1
while f_log_name.format(log_name_i) in f_log_names:
    log_name_i += 1
f_log_name = f_log_name.format(log_name_i)
print(f_log_name)
f_log = open("./TestLogs/" + f_log_name, "w+")

res = http.request("GET", host + "/api/GetLogin", fields={"username": "lqq", "password": "zf300122"})
print(res.data.decode())
token0 = eval(res.data.decode())["token"]

headers = {
    "content-type": "application/json-patch+json",
    "Authorization":
        "Bearer " + token0
}

res = http.request("GET", "http://192.168.21.24:25000/swagger/v1/swagger.json",
                   headers=headers)

data0 = [0]
with open("dom.txt", "w+") as f1:
    f1.write(res.data.decode())
with open("dom.txt", "r+") as f1:
    data0[0] = json.load(f1)

data0 = data0[0]
paths = data0["paths"]
# print(data0)

host = "http://192.168.21.24:25000"
data1 = {}
input0 = {}

for path in paths:
    if "get" in paths[path].keys():
        method0 = "get"
    if "post" in paths[path].keys():
        method0 = "post"
    tag0 = paths[path][method0]["tags"][0]

    if tag0 in ["File", "User", "WeatherForecast"]:
        continue

    # print(tag0,method0,path)
    if tag0 in data1.keys():
        if "summary" in paths[path][method0].keys():
            data1[tag0].append((tag0, method0, path, paths[path][method0]["summary"]))
        else:
            data1[tag0].append((tag0, method0, path, "NO SUMMARY"))


    else:
        if "summary" in paths[path][method0].keys():
            data1[tag0] = [(tag0, method0, path, paths[path][method0]["summary"])]
        else:
            data1[tag0] = [(tag0, method0, path, "NO SUMMARY")]

    # if path=="/api/Get":
    #     res=ul.urlopen(host+path)
    #
    #     print(res.read())

api_list0 = {}
api_list1 = {}
for tag in data1:
    tag_dict = {item[2]: (item[1], item[3], item[0]) for item in data1[tag]}
    for api in tag_dict:
        api_list0[api] = tag_dict[api]
        api_list1[api] = 0
        # print(api)

block_list = ['/api/UploadCheckPics/{checkid}',
              '/api/GetCheckPic/{checkid}/{index}',
              '/api/GetCheckPics/{checkid}',
              '/api/GetLogin', '/api/ShowLogin',
              '/api/UploadMessagePics',
              '/api/UploadMessageZip','/api/UploadNoticeZip', ]
for block_item in block_list:
    if block_item in api_list1:
        api_list1.pop(block_item)

if 0:
    template_simple_get = {
        """
        if 1 and "CategoryList":
            api = "sCategoryList"
            params = {
            }
            res = http.request(api_list0[api][0], host + api, fields=params, headers=headers)
            print(api_list0[api][2], api, api_list0[api][0], str(params), res.status, res.data.decode(), api_list0[api][1],
                  sep="\t")
        """
    }
    template_complex_get = {
        """
        if 1 and "":
            api = "essCategory"
            params = {
                "id": 7,
            }
            res = http.request(api_list0[api][0], host + api, fields=params, headers=headers)
            print(api_list0[api][2], api, api_list0[api][0], str(params), res.status, res.data.decode(), api_list0[api][1],
                  sep="\t")
        """
    }
    template_post = {
        """
        if 1 and "sCategory":
            api = "essCategory"
            params = {
                "id": 0,
                "code": "83",
                "addresscategory": "家庭常住住址"
            }
            paramsJ=json.dumps(params).encode('utf-8')
            res = http.request(api_list0[api][0], host + api, body=paramsJ, headers=headers)
            print(api_list0[api][2], api, api_list0[api][0], str(params), res.status, res.data.decode(), api_list0[api][1],
                  sep="\t")
        """
    }


def simple_get_test(api):
    res = http.request(api_list0[api][0], host + api, headers=headers)
    if res.status != 200 or eval(res.data.decode())["status"] != 200:
        print(api_list0[api][2], api, api_list0[api][0], str(""), res.status, res.data.decode()[0:10000],
              api_list0[api][1],
              sep="\t")
    api_list1[api] = 1

    f_log.write("{}\t{}\t{}\t{}\t{}\t{}\t{}\n".format(api_list0[api][2], api, api_list0[api][0], str(""), res.status,
                                                      res.data.decode()[0:10000], api_list0[api][1]))


def complex_get_test(api, params):
    res = http.request(api_list0[api][0], host + api, fields=params, headers=headers)
    if res.status != 200 or eval(res.data.decode())["status"] != 200:
        print(api_list0[api][2], api, api_list0[api][0], str(params), res.status, res.data.decode()[0:10000],
              api_list0[api][1],
              sep="\t")
    api_list1[api] = 1

    f_log.write(
        "{}\t{}\t{}\t{}\t{}\t{}\t{}\n".format(api_list0[api][2], api, api_list0[api][0], str(params), res.status,
                                              res.data.decode()[0:10000], api_list0[api][1]))


def post_test(api, params, need_insert_id=False, paramsJ=None):
    if paramsJ == None:
        paramsJ = json.dumps(params).encode('utf-8')
    res = http.request(api_list0[api][0], host + api, body=paramsJ, headers=headers)
    if res.status != 200 or eval(res.data.decode())["status"] != 200:
        print(api_list0[api][2], api, api_list0[api][0], str(params), res.status, res.data.decode()[0:10000],
              api_list0[api][1],
              sep="\t")

    f_log.write(
        "{}\t{}\t{}\t{}\t{}\t{}\t{}\n".format(api_list0[api][2], api, api_list0[api][0], str(params), res.status,
                                              res.data.decode()[0:10000], api_list0[api][1]))

    api_list1[api] = 1
    if need_insert_id:
        return int(eval(res.data.decode())["id"])


# addresscategory
if 1 and "AddressCategory" and "AF":
    insert_id = post_test("/api/SetAddressCategory", {
        "id": 0,
        "code": "434",
        "addresscategory": "自动测试地址A"
    }, True)
    post_test("/api/SetAddressCategory", {
        "id": insert_id,
        "code": "83",
        "addresscategory": "自动测试地址B"
    })
    simple_get_test("/api/GetAddressCategoryList")
    complex_get_test("/api/GetAddressCategory", {
        "id": 7
    })
    post_test("/api/DelAddressCategory", {
        "id": insert_id
    })
if "TODO" and 1 and "Appoint" and "F":
    complex_get_test("/api/GetOrgAppointList", {
        "orgid": 1
    })
    complex_get_test("/api/GetPersonAppointList", {
        "personid": 1
    })
    complex_get_test("/api/GetAppoint", {
        "id": 4
    })
    insert_id = post_test("/api/SetAppoint", {
        "id": 0,
        "orgnizationid": "1",
        "orgname": "测试组织",
        "personid": "1",
        "personname": "测试患者",
        "name": "NameAdd_To_Mod",
        "code": "CodeAdd_To_Mod",
        "vaccine": "VaccineAdd_To_Mod",
        "vaccinationdatestart": "0001-01-01 00:00:00",
        "vaccinationdateend": "0001-01-01 00:00:00",
        "injectiontimes": "0",
        "idcardno": "IDCardNOAdd_To_Mod",
        "tel": "TelAdd_To_Mod",
        "birthdate": "0001-01-01",
        "tstatus": "0",
        "appointmentcreatedtime": "0001-01-01 00:00:00",
        "iscancel": "0",
        "canceltime": "",
        "iscomplete": "0",
        "completetime": "",
        "description": ""
    }, True)
    post_test("/api/SetAppoint", {
        "id": insert_id,
        "orgnizationid": "1",
        "orgname": "测试组织",
        "personid": "1",
        "personname": "测试患者",
        "name": "NameAdd_To_Mod",
        "code": "CodeAdd_To_Mod",
        "vaccine": "VaccineAdd_To_Mod",
        "vaccinationdatestart": "0001-01-01 00:00:00",
        "vaccinationdateend": "0001-01-01 00:00:00",
        "injectiontimes": "0",
        "idcardno": "IDCardNOAdd_To_Mod",
        "tel": "TelAdd_To_Mod",
        "birthdate": "0001-01-01",
        "tstatus": "0",
        "appointmentcreatedtime": "0001-01-01 00:00:00",
        "iscancel": "0",
        "canceltime": "",
        "iscomplete": "0",
        "completetime": "",
        "description": ""
    })
    post_test("/api/DelAppoint", {
        "id": insert_id
    })
if 1 and "Area" and "AF":
    simple_get_test("/api/GetAreaList")
    complex_get_test("/api/GetAreaListH", {
        "parentid": 1787,
    })
    simple_get_test("/api/AreaList")
    complex_get_test("/api/GetArea", {
        "id": 1596,
    })
if 1 and "Attandent" and "AF":
    insert_id = "NaN"
    complex_get_test("/api/GetOrgAttandentList", {
        "orgid": 1,
    })
    complex_get_test("/api/GetPersonAttandentList", {
        "personid": 1
    })
    complex_get_test("/api/GetAttandent", {
        "id": 6,
    })
    insert_id = post_test("/api/SetAttandent",
                          {"id": "0", "personid": "1", "orgnizationid": "1", "srcorgid": "0", "desorgid": "0",
                           "admissiontime": "0001-01-01 00:00:00", "admissiontype": "AdmissionTypeADD",
                           "isdischarged": "0",
                           "dischargetime": "0001-01-01 00:00:00", "isreferral": "0", "desstatus": "DestStatus",
                           "destime": "0001-01-01 00:00:00", "isreferralcancel": "0", "isreferralfinish": "0",
                           "person": {"id": 1, "text": "测试患者", "code": "0000"},
                           "orgnization": {"id": 1, "text": "测试组织", "code": "0000", "register": "0000"}, "srcorg": {},
                           "desorg": {}, "status": 200, "msg": "读取成功"}, True)
    post_test("/api/SetAttandent",
              {"id": insert_id, "personid": "1", "orgnizationid": "1", "srcorgid": "0", "desorgid": "0",
               "admissiontime": "0001-01-01 00:00:00", "admissiontype": "AdmissionTypeADD", "isdischarged": "0",
               "dischargetime": "0001-01-01 00:00:00", "isreferral": "0", "desstatus": "DestStatus",
               "destime": "0001-01-01 00:00:00", "isreferralcancel": "0", "isreferralfinish": "0",
               "person": {"id": 1, "text": "测试患者", "code": "0000"},
               "orgnization": {"id": 1, "text": "测试组织", "code": "0000", "register": "0000"}, "srcorg": {},
               "desorg": {}, "status": 200, "msg": "读取成功"})
    post_test("/api/DelAttandent", {"id": insert_id})
if "TODO" and 0 and "Check" and "F":
    complex_get_test("/api/GetOrgCheckList", {
        "orgid": 1,
    })
    complex_get_test("/api/GetPersonCheckList", {
        "personid": 1,
    })
    complex_get_test("/api/GetCheck", {
        "id": 6
    })
    insert_id = post_test("/api/SetCheck", {
        "id": 0,
        "checktype": "测试类型A",
        "orgnizationid": 1,
        "orgname": "测试组织",
        "recommendedtreatid": 1,
        "recommend": "接种6支结核疫苗+定期筛查",
        "chosentreatid": 1,
        "chosen": "接种6支结核疫苗+定期筛查",
        "isreexam": true,
        "genderid": 1,
        "gendername": "未知的性别",
        "submittime": "2020-07-20 16:44:00",
        "orgcode": "0000",
        "detectionno": "",
        "pics": "D:\\upload\\55qeeehy.vox.png$$",
        "diagnoticstypeid": 2,
        "resultname": "阴性",
        "diagnoticstime": "2020-07-22 02:21:00",
        "diagnoticsby": "DiagnoticsByADD",
        "reporttime": "2020-07-23 02:05:00",
        "reportby": "ReportByADD",
        "reference": "ReferenceADD",
        "isactive": "0"
    }, True)
    post_test("/api/SetCheck", {
        "id": insert_id,
        "checktype": "测试类型B",
        "orgnizationid": 1,
        "orgname": "测试组织",
        "recommendedtreatid": 1,
        "recommend": "接种6支结核疫苗+定期筛查",
        "chosentreatid": 1,
        "chosen": "接种6支结核疫苗+定期筛查",
        "isreexam": true,
        "genderid": 1,
        "gendername": "未知的性别",
        "submittime": "2020-07-20 16:44:00",
        "orgcode": "0000",
        "detectionno": "",
        "pics": "D:\\upload\\55qeeehy.vox.png$$",
        "diagnoticstypeid": 2,
        "resultname": "阴性",
        "diagnoticstime": "2020-07-22 02:21:00",
        "diagnoticsby": "DiagnoticsByADD",
        "reporttime": "2020-07-23 02:05:00",
        "reportby": "ReportByADD",
        "reference": "ReferenceADD",
        "isactive": "0"
    })
    post_test("/api/DelCheck", {
        "id": insert_id
    })

    if 0 and "/api/GetCheckPics/{checkid}":
        api = "/api/GetCheckPics/{checkid}"
        params = {
            "id": 7,
        }
        res = http.request(api_list0[api][0], host + "/api/GetCheckPics/67", headers=headers)
        print(api_list0[api][2], api, api_list0[api][0], str({"checkid": 67}), res.status, res.data.decode(),
              api_list0[api][1],
              sep="\t")
    if 0 and "/api/GetCheckPic/{checkid}/{index}":
        api = "/api/GetCheckPic/{checkid}/{index}"
        # params = {
        #     "id": 7,
        # }
        res = http.request(api_list0[api][0], host + "/api/GetCheckPic/67/0", headers=headers)
        print(api_list0[api][2], api, api_list0[api][0], str({"checkid": 67, "index": 0}), res.status,
              api_list0[api][1],
              sep="\t")
if 1 and "CheckProduct" and "F":
    simple_get_test("/api/GetCheckProductListDefault")
    complex_get_test("/api/GetCheckProductList", {
        "pageSize": 10,
        "pageIndex": 1
    })
    complex_get_test("/api/GetCheckProduct", {
        "id": 95,
    })
    insert_id = post_test("/api/SetCheckProduct",
                          {"id": 0, "name": "NameAdd_To_ModMODITIED", "shortname": "ShortNameAdd_To_ModM",
                           "commonname": "CommonNameAdd_To_ModMODITIED", "specification": "SpecificationAdd_To_",
                           "manufacturer": "ManufacturerAdd_To_ModMODITIED", "esc": "ESCAdd_To_ModMODITIED",
                           "productiondate": "0001-01-01 00:00", "expirydate": "0001-01-01 00:00", "status": 200,
                           "msg": "读取成功"}, True)
    post_test("/api/SetCheckProduct",
              {"id": insert_id, "name": "NameAdd_To_ModMODITIED", "shortname": "ShortNameAdd_To_ModM",
               "commonname": "CommonNameAdd_To_ModMODITIED", "specification": "SpecificationAdd_To_",
               "manufacturer": "ManufacturerAdd_To_ModMODITIED", "esc": "ESCAdd_To_ModMODITIED",
               "productiondate": "0001-01-01 00:00", "expirydate": "0001-01-01 00:00", "status": 200, "msg": "读取成功"})
    post_test("/api/DelCheckProduct",
              {"id": insert_id, "name": "NameAdd_To_ModMODITIED", "shortname": "ShortNameAdd_To_ModM",
               "commonname": "CommonNameAdd_To_ModMODITIED", "specification": "SpecificationAdd_To_",
               "manufacturer": "ManufacturerAdd_To_ModMODITIED", "esc": "ESCAdd_To_ModMODITIED",
               "productiondate": "0001-01-01 00:00", "expirydate": "0001-01-01 00:00", "status": 200, "msg": "读取成功"})
if 1 and "DetectionResultType" and "F":
    simple_get_test("/api/GetDetectionResultTypeList")
    complex_get_test("/api/GetDetectionResultType", {
        "id": 3,
    })
    insert_id = post_test("/api/SetDetectionResultType", {"id": 0, "resultname": "测试结果A"}, True)
    post_test("/api/SetDetectionResultType", {"id": insert_id, "resultname": "测试结果B"})
    post_test("/api/DelDetectionResultType", {"id": insert_id})
if "TODO" and 1 and "DomiType" and "F":
    simple_get_test("/api/GetDomiTypeList")
    insert_id = post_test("/api/SetDomiType", {"id": 0, "name": "测试户籍类型A"}, True)
    post_test("/api/SetDomiType", {"id": insert_id, "name": "测试户籍类型B"})
    complex_get_test("/api/GetDomiType", {
        "id": insert_id,
    })
    post_test("/api/DelDomiType", {"id": insert_id})
if "TODO" and 0 and "File": pass
if "TODO" and 1 and "Followup":
    complex_get_test("/api/GetOrgFollowupList", {
        "orgid": 1
    })
    complex_get_test("/api/GetPersonFollowupList", {
        "personid": 1
    })
    complex_get_test("/api/GetFollowup", {
        "id": 28
    })
    insert_id = post_test("/api/SetFollowup", {
        "id": 0,
        "personid": 1,
        "personname": "测试患者",
        "personcode": "0000",
        "orgnizationid": 1,
        "orgname": "测试组织",
        "orgcode": "0000",
        "time": "0001-01-01 00:00:00",
        "personlist": "测试随访人员列表A",
        "abstract": "测试随访摘要A",
        "detail": "测试随访细节A"}, True)
    post_test("/api/SetFollowup", {
        "id": insert_id,
        "personid": 1,
        "personname": "测试患者",
        "personcode": "0000",
        "orgnizationid": 1,
        "orgname": "测试组织",
        "orgcode": "0000",
        "time": "0001-01-01 00:00:00",
        "personlist": "测试随访人员列表A",
        "abstract": "测试随访摘要A",
        "detail": "测试随访细节A"})

    post_test("/api/DelFollowup", {
        "id": insert_id
    })
if 1 and "Gender" and "F":
    simple_get_test("/api/GetGenderList")
    complex_get_test("/api/GetGender", {
        "id": 1,
    })
    insert_id = post_test("/api/SetGender", {"id": 0, "code": 8, "gendername": "测试性别A"}, True)
    post_test("/api/SetGender", {"id": insert_id, "code": 8, "gendername": "测试性别B"})
    post_test("/api/DelGender", {"id": insert_id})
if 1 and "IdCategory" and "F":
    simple_get_test("/api/GetIdCategoryList")
    complex_get_test("/api/GetIdCategory", {
        "id": 2,
    })
    insert_id = post_test("/api/SetIdCategory", {
        "id": 0,
        "code": "31",
        "name": "测试身份证件类型A"
    }, True)
    post_test("/api/SetIdCategory", {
        "id": insert_id,
        "code": "42",
        "name": "测试身份证件类型B"
    })
    post_test("/api/DelIdCategory", {
        "id": insert_id
    })
if False and "Login":
    pass
    if False and "/api/GetLogin":
        api = "/api/GetLogin"
        params = {
            "username": "lqq",
            "password": "zf300122"
        }
        res = http.request(api_list0[api][0], host + api, fields=params, headers=headers)
        print(api_list0[api][2], api, api_list0[api][0], str(params), res.status, res.data.decode(), api_list0[api][1],
              sep="\t")
if 1 and "Medication" and "F":
    simple_get_test("/api/GetMedicationListD")
    complex_get_test("/api/GetMedicationList", {
        "pageSize": 10,
        "pageIndex": 1
    })
    complex_get_test("/api/GetMedication", {
        "id": 3726,
    })
    insert_id = post_test("/api/SetMedication", {
        "id": 0,
        "name": "TESTMedicationNameA",
        "commonname": "TESTMedicationCommonNameA",
        "specification": "SpecificationAdd_To_",
        "esc": "ESC9346cf26-0fca-4617-810c-" + str(random.randrange(10000000000, 99999999999)) + "fAdd_To_Mod",
        "productiondate": "2020-07-02 10:54:02",
        "expirydate": "2022-07-02 10:54:02",
        "manufacturer": "ManufacturerAdd_To_Mod"
    }, True)
    post_test("/api/SetMedication", {
        "id": insert_id,
        "name": "TESTMedicationNameB",
        "commonname": "TESTMedicationCommonNameB",
        "specification": "SpecificationAdd_To_",
        "esc": "ESC9346cf26-0fca-4617-810c-" + str(random.randrange(10000000000, 99999999999)) + "fAdd_To_Mod",
        "productiondate": "2020-07-02 10:54:02",
        "expirydate": "2022-07-02 10:54:02",
        "manufacturer": "ManufacturerAdd_To_Mod"
    })
    post_test("/api/DelMedication", {
        "id": insert_id
    })
if 1 and "MedicationDosageForm" and "F":
    simple_get_test("/api/GetMedicationDosageFormList")
    complex_get_test("/api/GetMedicationDosageForm", {
        "id": 3,
    })
    insert_id = post_test("/api/SetMedicationDosageForm", {
        "id": 0,
        "code": "58",
        "name": "测试药物剂型A"
    }, True)
    post_test("/api/SetMedicationDosageForm", {
        "id": insert_id,
        "code": "67",
        "name": "测试药物剂型B"
    })
    post_test("/api/DelMedicationDosageForm", {
        "id": insert_id
    })
if 1 and "MedicationFreqCategory" and "F":
    simple_get_test("/api/GetMedicationFreqCategoryList")
    complex_get_test("/api/GetMedicationFreqCategory", {
        "id": 3,
    })
    insert_id = post_test("/api/SetMedicationFreqCategory", {
        "id": 0,
        "code": "83",
        "value": "testA",
        "valuemessage": "测试用药频率A"
    }, True)
    post_test("/api/SetMedicationFreqCategory", {
        "id": insert_id,
        "code": "36",
        "value": "testB",
        "valuemessage": "测试用药频率B"
    })
    post_test("/api/DelMedicationFreqCategory", {
        "id": insert_id
    })
if 1 and "MedicationPathway" and "F":
    simple_get_test("/api/GetMedicationPathwayList")
    complex_get_test("/api/GetMedicationPathway", {
        "id": 5,
    })
    insert_id = post_test("/api/SetMedicationPathway", {
        "id": 0,
        "code": "35",
        "name": "测试用药方法A",
        "introduction": "将药物置于测试程序中的给药方法A"
    }, True)
    post_test("/api/SetMedicationPathway", {
        "id": insert_id,
        "code": "48",
        "name": "测试用药方法B",
        "introduction": "将药物置于测试程序中的给药方法B"
    })
    post_test("/api/DelMedicationPathway", {
        "id": insert_id
    })
if "TODO" and 1 and "Menu":
    complex_get_test("/api/GetMenu", {
        "pid": 2
    })
    simple_get_test("/api/GetMenuList")
    complex_get_test("/api/GetMenuId", {
        "id": 12
    })
    insert_id = post_test("/api/SetMenu", {
        "id": 0,
        "name": "test_menuA",
        "icon": "test_iconA",
        "label": "测试菜单A",
        "pid": 0,
        "seq": 0,
        "children": []
    }, True)
    post_test("/api/SetMenu", {
        "id": insert_id,
        "name": "test_menuB",
        "icon": "test_iconB",
        "label": "测试菜单B",
        "pid": 0,
        "seq": 0,
        "children": []
    })
    post_test("/api/DelMenu", {
        "id": insert_id
    })
if "TODO" and 1 and "Message":
    simple_get_test("/api/GetMessageList")
    complex_get_test("/api/GetMessage", {
        "id": 28
    })
    insert_id = post_test("/api/SetMessage", {
        "id": 0,
        "orgnizationid": "1",
        "orgname": "测试组织",
        "publishuserid": "4",
        "publish": "开发小组",
        "publishtime": "0001-01-01 00:00:00",
        "title": "测试公告标题A",
        "abstract": "",
        "thumbnail": "",
        "content": "测试公告内容A",
        "attachment": "",
        "ispublic": "1",
        "isactive": "0"
    }, True)
    post_test("/api/SetMessage", {
        "id": insert_id,
        "orgnizationid": "1",
        "orgname": "测试组织",
        "publishuserid": "4",
        "publish": "开发小组",
        "publishtime": "0001-01-01 00:00:00",
        "title": "测试公告标题B",
        "abstract": "",
        "thumbnail": "",
        "content": "测试公告内容B",
        "attachment": "",
        "ispublic": "1",
        "isactive": "0"
    })
    post_test("/api/DelMessage", {
        "id": insert_id
    })
if 1 and "Nation" and "F":
    simple_get_test("/api/GetNationList")
    complex_get_test("/api/GetNation", {
        "id": 41,
    })
    insert_id = post_test("/api/SetNation", {
        "id": 0,
        "name": "测试民族A"
    }, True)
    post_test("/api/SetNation", {
        "id": insert_id,
        "name": "测试民族A"
    })
    post_test("/api/DelNation", {
        "id": insert_id
    })
if 1 and "Notice" and "F":
    simple_get_test("/api/GetNoticeList")
    complex_get_test("/api/GetNotice", {
        "id": 1,
    })
    insert_id = post_test("/api/SetNotice", {
        "id": 0,
        "orgnizationid": "1",
        "orgname": "测试组织",
        "publishuserid": "4",
        "publish": "开发小组",
        "publishtime": "",
        "content": "<p>测试内容A</p>\n",
        "attachment": ""
    }, True)
    post_test("/api/SetNotice", {
        "id": insert_id,
        "orgnizationid": "1",
        "orgname": "测试组织",
        "publishuserid": "4",
        "publish": "开发小组",
        "publishtime": "",
        "content": "<p>测试内容B</p>\n",
        "attachment": ""
    })
    post_test("/api/DelNotice", {
        "id": insert_id
    })
if "TODO" and 1 and "Occupation":
    simple_get_test("/api/GetOccupationList")
    complex_get_test("/api/GetOccupation", {
        "id": 10
    })
    insert_id = post_test("/api/SetOccupation", {
        "id": 0,
        "code": "82",
        "occupationname": "测试职业A",
        "isactive": True
    }, True)
    post_test("/api/SetOccupation", {
        "id": insert_id,
        "code": "87",
        "occupationname": "测试职业B",
        "isactive": True
    })
    post_test("/api/DelOccupation", {
        "id": insert_id
    })

if 1 and "Option" and "F":
    simple_get_test("/api/GetOptionListD")
    complex_get_test("/api/GetOptionList", {
        "section": ""
    })
    complex_get_test("/api/GetOption", {
        "id": 1
    })
    insert_id = post_test("/api/SetOption", {
        "id": 0,
        "section": "test_section",
        "name": "test_option_12",
        "value": "TrueOrFalse",
        "description": "An option for test."
    }, True)
    post_test("/api/SetOption", {
        "id": insert_id,
        "section": "test_section",
        "name": "test_option_17",
        "value": "ToBeOrNotToBe",
        "description": "Again, an option for test."
    })
    post_test("/api/DelOption", {
        "id": insert_id
    })
if "TODO" and 1 and "Orgnization":
    simple_get_test("/api/GetOrgListD")
    complex_get_test("/api/GetOrgList",{
        "pageSize":10,
        "pageIndex":1
    })
    complex_get_test("/api/GetOrgListv2",{
      "provinceid": 1589,
      "cityid": 1590,
      "countyid": 1591
    })
    insert_id=post_test("/api/SetOrg",{
      "id": 0,
      "orgname": "测试组织名A",
      "orgcode": "testocA",
      "certcode": "testccA",
      "legalname": "testlnA",
      "legalidcode": "testlicA",
      "address": "testaddrA",
      "tel": "testtelA",
      "coordinates": "testcoA",
      "parentid": "0",
      "parent": "swagger",
      "provinceid": "1589",
      "province": "北京市",
      "cityid": "1590",
      "city": "北京市",
      "countyid": "1591",
      "county": "东城区",
      "isactive": "1"
    },True)
    post_test("/api/SetOrg", {
        "id": insert_id,
        "orgname": "测试组织名B",
        "orgcode": "testocB",
        "certcode": "testccB",
        "legalname": "testlnB",
        "legalidcode": "testlicB",
        "address": "testaddrB",
        "tel": "testtelB",
        "coordinates": "testcoB",
        "parentid": "0",
        "parent": "swagger",
        "provinceid": "1589",
        "province": "北京市",
        "cityid": "1590",
        "city": "北京市",
        "countyid": "1591",
        "county": "东城区",
        "isactive": "1"
    })
    complex_get_test("/api/GetOrg",{
        "id":insert_id
    })
    post_test("/api/DelOrg",{
        "id":insert_id
    })
if "TODO" and 1 and "Person":
    simple_get_test("/api/GetPersonListD")
    complex_get_test("/api/GetPersonList",{
        "pageSize":10,
        "pageIndex":1
    })
    insert_id=post_test("/api/SetPerson",{
      "id": 0,
      "isreferral": "0",
      "orgnizationid": "1",
      "primaryorgnizationid": "1",
      "orgname": "测试组织",
      "orgcode": "0000",
      "registerno": "",
      "familyname": "测试患者",
      "tel": "13123123",
      "idcardno": random.randrange(1000,9999),
      "genderid": "1",
      "gendername": "未知的性别",
      "birthday": "2020-07-01",
      "nation": "1",
      "domiciletype": "1",
      "domiciledetail": "123",
      "workunitname": "华新街",
      "occupationcategoryid": "1",
      "occupationname": "国家公务员",
      "detainees": "111",
      "addresscategoryid": "1",
      "addresscategory": "户籍地址",
      "address": "华新街",
      "guardianname": "华新街",
      "guardiancontact": "1582222",
      "provinceid": "1608",
      "province": "香港特别行政区",
      "cityid": "4965",
      "city": "A市",
      "countyid": "4966",
      "county": "A市1区",
      "isactive": "1"
    },True)
    post_test("/api/SetPerson", {
        "id": insert_id,
        "isreferral": "0",
        "orgnizationid": "1",
        "primaryorgnizationid": "1",
        "orgname": "测试组织",
        "orgcode": "0000",
        "registerno": "",
        "familyname": "测试患者",
        "tel": "13123123",
        "idcardno": "0000",
        "genderid": "1",
        "gendername": "未知的性别",
        "birthday": "2020-07-01",
        "nation": "1",
        "domiciletype": "1",
        "domiciledetail": "123",
        "workunitname": "华新街",
        "occupationcategoryid": "1",
        "occupationname": "国家公务员",
        "detainees": "111",
        "addresscategoryid": "1",
        "addresscategory": "户籍地址",
        "address": "华新街",
        "guardianname": "华新街",
        "guardiancontact": "1582222",
        "provinceid": "1608",
        "province": "香港特别行政区",
        "cityid": "4965",
        "city": "A市",
        "countyid": "4966",
        "county": "A市1区",
        "isactive": "1"
    })
    complex_get_test("/api/GetPerson", {
        "id": insert_id
    })
    post_test("/api/DelPerson",{
        "id":insert_id
    })
if "TODO" and 0 and "Treat":
    complex_get_test("/api/GetOrgTreatList2",{
        "orgid":1
    })
    simple_get_test("/api/GetOrgTreatList")
    complex_get_test("/api/GetPersonTreatList2",{
        "personid":1
    })
    complex_get_test("/api/GetPersonTreatList", {
        "personid": 1
    })
    complex_get_test("/api/GetTreat", {
        "id": 136
    })
    insert_id=post_test("/api/SetTreat",{
      "id": 0,
      "orgnizationid": "",
      "orgname": "",
      "orgcode": "",
      "prescriptioncode": "PrescriptionCodeLTBIBLLTests.TreatBLLTests.ModifyBatchTest71Modify",
      "medicationid": "",
      "medicationname": "",
      "personid": "",
      "personname": "",
      "personidcard": "",
      "diseasecode": "DiseaseCodeLTBIBLLTests.TreatBLLTests.ModifyBatchT",
      "treatname": "TreatNameLTBIBLLTests.TreatBLLTests.ModifyBatchTest71Modify",
      "druggroupnumber": "",
      "tstatus": "",
      "prescriber": "PrescriberLTBIBLLTests.TreatBLLTests.ModifyBatchTest71Modify",
      "prescribetime": "",
      "prescribedepartment": "PrescribeDepartmentLTBIBLLTests.TreatBLLTests.ModifyBatchTest71Modify",
      "iscancel": "",
      "canceltime": "",
      "completetime": "",
      "medicationdosageformid": "",
      "dosage": "",
      "medicationfreqcategoryid": "",
      "freq": "",
      "medicationpathwayid": "",
      "pathway": "",
      "singledoseamount": "",
      "singledoseunit": "",
      "totaldoseamount": "",
      "isactive": "1"
    },True)
    post_test("/api/SetTreat", {
        "id": insert_id,
        "orgnizationid": "",
        "orgname": "",
        "orgcode": "",
        "prescriptioncode": "PrescriptionCodeLTBIBLLTests.TreatBLLTests.ModifyBatchTest71Modify",
        "medicationid": "",
        "medicationname": "",
        "personid": "",
        "personname": "",
        "personidcard": "",
        "diseasecode": "DiseaseCodeLTBIBLLTests.TreatBLLTests.ModifyBatchT",
        "treatname": "TreatNameLTBIBLLTests.TreatBLLTests.ModifyBatchTest71Modify",
        "druggroupnumber": "",
        "tstatus": "",
        "prescriber": "PrescriberLTBIBLLTests.TreatBLLTests.ModifyBatchTest71Modify",
        "prescribetime": "",
        "prescribedepartment": "PrescribeDepartmentLTBIBLLTests.TreatBLLTests.ModifyBatchTest71Modify",
        "iscancel": "",
        "canceltime": "",
        "completetime": "",
        "medicationdosageformid": "",
        "dosage": "",
        "medicationfreqcategoryid": "",
        "freq": "",
        "medicationpathwayid": "",
        "pathway": "",
        "singledoseamount": "",
        "singledoseunit": "",
        "totaldoseamount": "",
        "isactive": "1"
    })
    post_test("/api/DelTreat", {
        "id": insert_id
    })
if 1 and "TreatmentOption" and "F":
    simple_get_test("/api/GetTreatmentOptionList")
    complex_get_test("/api/GetTreatmentOption", {
        "id": 5,
    })
    insert_id = post_test("/api/SetTreatmentOption", {
        "id": 0,
        "name": "测试治疗方案A"
    }, True)
    post_test("/api/SetTreatmentOption", {
        "id": insert_id,
        "name": "测试治疗方案B"
    })
    post_test("/api/DelTreatmentOption", {
        "id": insert_id
    })
if "TODO" and 0 and "User":
    if 1 and "/api/User/GetBaseData":
        api = "/api/User/GetBaseData"
        params = {
        }
        res = http.request(api_list0[api][0], host + api, fields=params, headers=headers)
        print(api_list0[api][2], api, api_list0[api][0], str(params), res.status, res.data.decode(), api_list0[api][1],
              sep="\t")
    if 1 and "/api/User/{id}":
        api = "/api/User/{id}"
        params = {
            "id": 2890
        }
        res = http.request(api_list0[api][0], host + "/api/User/{0}".format(2890), headers=headers)
        print(api_list0[api][2], api, api_list0[api][0], str(params), res.status, res.data.decode(), api_list0[api][1],
              sep="\t")
    if 1 and "/api/Set" + GGSDkey:
        api = "/api/Set" + GGSDkey
        params = params_set_update
        paramsJ = json.dumps(params).encode('utf-8')
        res = http.request(api_list0[api][0], host + api, body=paramsJ, headers=headers)
        print(api_list0[api][2], api, api_list0[api][0], str(params), res.status, res.data.decode(), api_list0[api][1],
              sep="\t")
    if 1 and "/api/Set" + GGSDkey:
        api = "/api/Set" + GGSDkey
        params = params_set_insert
        paramsJ = json.dumps(params).encode('utf-8')
        res = http.request(api_list0[api][0], host + api, body=paramsJ, headers=headers)
        print(api_list0[api][2], api, api_list0[api][0], str(params), res.status, res.data.decode(), api_list0[api][1],
              sep="\t")
    if 1 and "/api/Del" + GGSDkey:
        api = "/api/Del" + GGSDkey
        params = params_set_update
        paramsJ = json.dumps(params).encode('utf-8')
        res = http.request(api_list0[api][0], host + api, body=paramsJ, headers=headers)
        print(api_list0[api][2], api, api_list0[api][0], str(params), res.status, res.data.decode(), api_list0[api][1],
              sep="\t")
if 1 and "Vacc":
    simple_get_test("/api/GetOrgVaccList")
    complex_get_test("/api/GetPersonVaccList",{
        "personid":1
    })
    complex_get_test("/api/GetVacc",{
        "id":81
    })
    insert_id=post_test("/api/SetVacc",{
      "id": 0,
      "patientid": 1,
      "person": "测试患者",
      "orgnizationid": 1,
      "orgname": "测试组织",
      "operationuserid": 4,
      "operator": "开发小组",
      "medicationid": 7316,
      "medication": "测试药物A",
      "commonname": "测试名称B",
      "medicationdosageformid": 1,
      "dosage": "原料",
      "medicationpathwayid": 2,
      "pathway": "直肠用药",
      "operationtime": "0001-01-01 00:00:00",
      "leavetime": "0001-01-01 00:00:00",
      "nexttime": "0001-01-01 00:00:00",
      "fstatus": "StatusAdd_To_Mod",
      "tempraturep": 0,
      "tempraturen": 0,
      "effect": "EffectAdd_To_Mod",
      "isactive": false
    },True)
    post_test("/api/SetVacc", {
        "id": insert_id,
        "patientid": 1,
        "person": "测试患者",
        "orgnizationid": 1,
        "orgname": "测试组织",
        "operationuserid": 4,
        "operator": "开发小组",
        "medicationid": 7316,
        "medication": "测试药物A",
        "commonname": "测试名称B",
        "medicationdosageformid": 1,
        "dosage": "原料",
        "medicationpathwayid": 2,
        "pathway": "直肠用药",
        "operationtime": "0001-01-01 00:00:00",
        "leavetime": "0001-01-01 00:00:00",
        "nexttime": "0001-01-01 00:00:00",
        "fstatus": "StatusAdd_To_Mod",
        "tempraturep": 0,
        "tempraturen": 0,
        "effect": "EffectAdd_To_Mod",
        "isactive": false
    })
    post_test("/api/DelVacc", {
        "id": insert_id
    })


# {"id": 0,"name": "set","icon": "","label": "pid": 3,"seq": 0,"children": []}人员信息编辑","
print([api for api in api_list1 if api_list1[api] == 0])
