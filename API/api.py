from flask import Flask, request
from flask_cors import CORS
from flask_restful import Resource, Api
import time
from threading import Thread
import pandas as pd
from collections import Counter
import pyodbc
import json

app = Flask(__name__)
api = Api(app)
CORS(app)

#DB Connector Initialization:    
cnxn = pyodbc.connect('DRIVER={ODBC Driver 17 for SQL Server};SERVER="";DATABASE=superpower;UID="";PWD="";MARS_Connection=yes', autocommit=True)
cursor = cnxn.cursor()

######## API RESOURCE CLASSES ########

class Test(Resource):
    def get(self):
        return {'Test Message(GET)': 'Welcome to new API !!'}
    
    def post(self):
        return {'Test Message(POST)': 'Welcome to new API !!'}

#Returns : exists
class UserLogin(Resource):
    def post(self):
        data = request.get_json()
        email = data['email']
        password = data['password'] 
        
        uId = cursor.execute("{call userCheck(?,?)}", (email, password)).fetchval()
        if(uId != None and uId != 0):
            return {'exists': True}
        else:
            return {'exists': False}
        
#Returns : successful
class UserRegister(Resource):
    def post(self):
        data = request.get_json()
        uname = data['uname']
        countryName = data['cname']
        email = data['email']
        password = data['password']
        
        sql = """\
        DECLARE @rv int;
        EXEC @rv = userRegister @uname=?, @cname=?, @email=?, @password=?
        SELECT @rv AS return_value;
        """
        params = (uname, countryName, email, password)
        res = cursor.execute(sql, params).fetchval()
        if res > 0:
            return {'successful': True} 
        else:
            return {'successful': False}
            
#Returns : successful       
class ForgotPassword(Resource):
    def post(self):
        data = request.get_json()
        email = data['email']

        return {'successful': True}
    
#Returns : cname, totalPopulation, avgTaxRate, #ofProvinces, lastYearsIncome, remainingOfIncome statusWith
class CountriesDetails(Resource):
    def post(self):
        data = request.get_json()
        email = data['email']
        password = data['password']
     
        rows = cursor.execute("{call countriesDetails(?,?)}", (email, password)).fetchall()
        dataList = []
        for row in rows:
            data_as_dict = {
                "id" : row[0],
                "cname" : row[1],
                "population" : row[2],
                "taxRate": row[3],
                "provinceNum": row[4],
                "income": row[5],
                "remaining": row[6]
                }
            dataList.append(data_as_dict)
            
        return json.dumps(dataList)

#Params : countryId          
#Returns : pname, population, taxRate, militaryCorpsInProvince, quantityOfResources, quantityOfProducts, typesOfInvestments, lastBudgetAmount, lastBudgetRemaining
class ProvincesDetails(Resource):
    def post(self):
        data = request.get_json()
        email = data['email']
        password = data['password']
        countryID = data['countryId']
        year = data['year']
        
        rows = cursor.execute("{call provincesDetails(?,?,?,?)}", (email, password, countryID, year)).fetchall()
        dataList = []
        for row in rows:
            data_as_dict = {
                "name" : row[0],
                "population" : row[1],
                "taxRate" : row[2],
                "corpsInProvince": row[3],
                "resources": row[4],
                "products": row[5],
                "investments": row[6],
                "budgetDetails": row[7]
                }
            dataList.append(data_as_dict)
            
        return json.dumps(dataList)
            
    
#Deprecated in 14.12.2018
#Returns : ...
class MyCountryDetails(Resource):
    def post(self):
        data = request.get_json()
        email = data['email']
        password = data['password']
        
        return {}
    
#Returns : My army informations - armyBudget, List<armyCorps,opearation>
class ArmyInformations(Resource):
    def post(self):
        data = request.get_json()
        email = data['email']
        password = data['password']
        countryID = data['countryId']
        
        rows = cursor.execute("{call armyInformations(?,?,?)}", (email, password, countryID)).fetchall()
        dataList = []
        for row in rows:
            data_as_dict = {
                "id" : row[0],
                "budget" : row[1],
                "corpDetails" : row[2]
                }
            dataList.append(data_as_dict)
            
        return json.dumps(dataList)

#Params : corpId, targetProvinceId, mission
#Returns : successful
class GiveMissionToCorps(Resource):
    def post(self):
        data = request.get_json()
        email = data['email']
        password = data['password']
        corpID = ['corpId']
        provinceID = ['provinceId']
        mission = ['mission']
            
        
        success = cursor.execute("{call giveMissionToCorps(?,?,?,?,?)}", (email, password, corpID, provinceID, mission)).fetchval()
        if(success >= 0):
            return {'successful': True}
        else:
            return {'successful': False}
        
        
#Params : corpId,missionId
#Returns : successful
#Definition : Gives corps a new mission about going back to where they came
class AbortMissionOfCorp(Resource):
    def post(self):
        data = request.get_json()
        email = data['email']
        password = data['password']
        corpID = ['corpId']
        missionID = ['missionId']

        success = cursor.execute("{call abortMissionOfCorp(?,?,?,?)}", (email, password, corpID, missionID)).fetchval()
        if(success >= 0):
            return {'successful': True}
        else:
            return {'successful': False}
        

#Returns : rows in countryAggrements table related to users country
class AggrementsInformations(Resource):
    def post(self):
        data = request.get_json()
        email = data['email']
        password = data['password']
        countryID = data['countryId']

        rows = cursor.execute("{call aggrementsInformations(?,?,?)}", (email, password, countryID)).fetchall()
        dataList = []
        for row in rows:
            data_as_dict = {
                "c1Id" : row[0],
                "c2Id" : row[1],
                "aggrementId" : row[2],
                "endDate": row[3]
                }
            dataList.append(data_as_dict)
            
        return json.dumps(dataList)
        
    
#Params : c1Id, c2Id, aggrementId
#Returns : successful
class DeclineAggrement(Resource):
    def post(self):
        data = request.get_json()
        email = data['email']
        password = data['password']
        c1ID = data['c1Id']
        c2ID = data['c2Id']
        aggrementID = data['aggrementId']

        success = cursor.execute("{call declineAggrement(?,?,?,?,?)}", (email, password, c1ID, c2ID, aggrementID)).fetchval()
        if(success >= 0):
            return {'successful': True}
        else:
            return {'successful': False}
        

#Params : c1Id, c2Id, aggrementId, endDate
#Returns : successful
class OfferAggrement(Resource):
    def post(self):
        data = request.get_json()
        email = data['email']
        password = data['password']
        c1ID = data['c1Id']
        c2ID = data['c2Id']
        aggrementID = data['aggrementId']
        endDate = data['endDate']

        success = cursor.execute("{call offerAggrement(?,?,?,?,?,?)}", (email, password, c1ID, c2ID, aggrementID, endDate)).fetchval()
        if(success >= 0):
            return {'successful': True}
        else:
            return {'successful': False}
        

#Params : c1Id, c2Id, aggrementId, answer
#Returns : successful
class AnswerAggrementOffer(Resource):
    def post(self):
        data = request.get_json()
        email = data['email']
        password = data['password']
        c1ID = data['c1Id']
        c2ID = data['c2Id']
        aggrementID = data['aggrementId']
        endDate = data['endDate']
        answer = data['answer']

        success = cursor.execute("{call answerAggrementOffer(?,?,?,?,?,?,?)}", (email, password, c1ID, c2ID, aggrementID, endDate, answer)).fetchval()
        if(success >= 0):
            return {'successful': True}
        else:
            return {'successful': False}
    
#Returns : rows in aggrementOffers table related to users country
class AggrementOfferInformations(Resource):
    def post(self):
        data = request.get_json()
        email = data['email']
        password = data['password']
        countryID = data['countryId']

        rows = cursor.execute("{call aggrementOfferInformations(?,?,?)}", (email, password, countryID)).fetchall()
        dataList = []
        for row in rows:
            data_as_dict = {
                "c1Id" : row[0],
                "c2Id" : row[1],
                "aggrementId" : row[2],
                "endDate": row[3]
                }
            dataList.append(data_as_dict)
            
        return json.dumps(dataList)
    
    
#Returns : rows in countryRegulations table related to users country
class RegulationsInformations(Resource):
    def post(self):
        data = request.get_json()
        email = data['email']
        password = data['password']
        countryID = data['countryId']

        rows = cursor.execute("{call regulationsInformations(?,?,?)}", (email, password, countryID)).fetchall()
        dataList = []
        for row in rows:
            data_as_dict = {
                "rId" : row[1],
                "startDate" : row[2],
                "endDate" : row[3]
                }
            dataList.append(data_as_dict)
            
        return json.dumps(dataList)
    
    
#Returns : rows in countryLaws table related to users country
class LawsInformations(Resource):
    def post(self):
        data = request.get_json()
        email = data['email']
        password = data['password']
        countryID = data['countryId']

        rows = cursor.execute("{call lawsInformations(?,?,?)}", (email, password, countryID)).fetchall()
        dataList = []
        for row in rows:
            data_as_dict = {
                "rId" : row[1],
                "startDate" : row[2],
                "endDate" : row[3]
                }
            dataList.append(data_as_dict)
            
        return json.dumps(dataList)
    
    
#Params : cId, rId
#Returns : successful
class DeclineRegulation(Resource):
    def post(self):
        data = request.get_json()
        email = data['email']
        password = data['password']
        countryID = data['countryId']
        regulationID = data['regulationId']

        success = cursor.execute("{call declineRegulation(?,?,?,?)}", (email, password, countryID, regulationID)).fetchval()
        if(success >= 0):
            return {'successful': True}
        else:
            return {'successful': False}
        
    
#Params : cId, lId
#Returns : successful
class DeclineLaw(Resource):
    def post(self):
        data = request.get_json()
        email = data['email']
        password = data['password']
        countryID = data['countryId']
        lawID = data['lawId']

        success = cursor.execute("{call declineLaw(?,?,?,?)}", (email, password, countryID, lawID)).fetchval()
        if(success >= 0):
            return {'successful': True}
        else:
            return {'successful': False}


#Params : cId, rId, endDate
#Returns : successful
class MakeRegulation(Resource):
    def post(self):
        data = request.get_json()
        email = data['email']
        password = data['password']
        countryID = data['countryId']
        regulationID = data['regulationId']
        endDate = data['endDate']

        success = cursor.execute("{call makeRegulation(?,?,?,?,?)}", (email, password, countryID, regulationID, endDate)).fetchval()
        if(success >= 0):
            return {'successful': True}
        else:
            return {'successful': False}
    
#Params : cId, lId
#Returns : successful
class MakeLaw(Resource):
    def post(self):
        data = request.get_json()
        email = data['email']
        password = data['password']

        countryID = data['countryId']
        lawID = data['lawId']
        endDate = data['endDate']

        success = cursor.execute("{call makeLaw(?,?,?,?,?)}", (email, password, countryID, lawID, endDate)).fetchval()
        if(success >= 0):
            return {'successful': True}
        else:
            return {'successful': False}
        
    
#Returns : users provinces last bugdets
class BudgetInformations(Resource):
    def post(self):
        data = request.get_json()
        email = data['email']
        password = data['password']
        countryID = data['countryId']

        rows = cursor.execute("{call budgetInformations(?,?,?)}", (email, password, countryID)).fetchall()
        dataList = []
        for row in rows:
            data_as_dict = {
                "provinceID" : row[0],
                "amount" : row[1],
                "remaining" : row[2],
                "year" : row[3]
                }
            dataList.append(data_as_dict)
            
        return json.dumps(dataList)
    
    
#Params : provinceId, amount, year
#Returns : successful
class SetBudgetForProvince(Resource):
    def post(self):
        data = request.get_json()
        email = data['email']
        password = data['password']
        provinceID = ['provinceId']
        year = data['year']
        amount = data['amount']

        success = cursor.execute("{call setBudgetForProvince(?,?,?,?,?)}", (email, password, provinceID, year, amount)).fetchval()
        if(success >= 0):
            return {'successful': True}
        else:
            return {'successful': False}
        
    
#Params : amount
#Returns : successful
class SetBudgetForArmy(Resource):
    def post(self):
        data = request.get_json()
        email = data['email']
        password = data['password']
        amount = ['amount']
        countryID = data['countryId']

        success = cursor.execute("{call setBudgetForArmy(?,?,?,?,?)}", (email, password, amount, countryID)).fetchval()
        if(success >= 0):
            return {'successful': True}
        else:
            return {'successful': False}
        

#Params : provinceId, taxRate
#Returns : successful
class SetTaxRateForProvince(Resource):
    def post(self):
        data = request.get_json()
        email = data['email']
        password = data['password']
        provinceID = data['provinceId']
        taxRate = data['taxRate']

        success = cursor.execute("{call setTaxRateForProvince(?,?,?,?)}", (email, password, provinceID, taxRate)).fetchval()
        if(success >= 0):
            return {'successful': True}
        else:
            return {'successful': False}
        
    
#Params : provinceId, investmentId, degree
#Returns : successful
class MakeInvestment(Resource):
    def post(self):
        data = request.get_json()
        email = data['email']
        password = data['password']
        provinceID = data['provinceId']
        investmentID = data['investmentId']
        degree = data['degree']
        
        success = cursor.execute("{call makeInvestment(?,?,?,?)}", (email, password, provinceID, investmentID, degree)).fetchval()
        if(success >= 0):
            return {'successful': True}
        else:
            return {'successful': False}
        
    
#######################################
        

api.add_resource(Test, '/test')

api.add_resource(UserLogin, '/userLogin')  
api.add_resource(UserRegister, '/userRegister')
api.add_resource(ForgotPassword, '/forgotPassword')

api.add_resource(CountriesDetails, '/countriesDetails')
api.add_resource(ProvincesDetails, '/provincesDetails')
api.add_resource(MyCountryDetails, '/myCountryDetails')

api.add_resource(ArmyInformations, '/armyInformations')
api.add_resource(GiveMissionToCorps, '/giveMissionToCorps')
api.add_resource(AbortMissionOfCorp, '/abortMissionOfCorp')

api.add_resource(AggrementsInformations, '/aggrementsInformations')
api.add_resource(AggrementOfferInformations, '/aggrementOfferInformations')
api.add_resource(OfferAggrement, '/offerAggrement')
api.add_resource(DeclineAggrement, '/declineAggrement')
api.add_resource(AnswerAggrementOffer, '/answerAggrementOffer')

api.add_resource(RegulationsInformations, '/regulationsInformations')
api.add_resource(LawsInformations, '/lawsInformations')
api.add_resource(DeclineRegulation, '/declineRegulation')
api.add_resource(DeclineLaw, '/declineLaw')
api.add_resource(MakeRegulation, '/makeRegulation')
api.add_resource(MakeLaw, '/makeLaw')

api.add_resource(BudgetInformations, '/budgetInformations')
api.add_resource(SetBudgetForProvince, '/setBudgetForProvince')
api.add_resource(SetBudgetForArmy, '/setBudgetForArmy')
api.add_resource(SetTaxRateForProvince, '/setTaxRateForProvince')

api.add_resource(MakeInvestment, '/makeInvestment')



if __name__ == '__main__':
    app.run(debug=True, use_reloader=True)
    
