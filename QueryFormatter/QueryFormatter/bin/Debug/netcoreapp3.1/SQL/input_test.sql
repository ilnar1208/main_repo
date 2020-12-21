select

f.xMest xMest,

f.XLicMest XLicMest,

f.xLic xLic,

f.xDick xDick,

f.xDobito xDobito,

f.xPoteri xPoteri,

f.XNorm XNorm, -- tpmkj

f.xCode xCode,

f.xdobito2 xdobito2,

f.xpoteri2 xpoteri2,

(f.xdobito - f.xdobito2) xdobito3,

(f.xpoteri - f.xpoteri2) xpoteri3,

f.uch1 uch1,

f.uch2 uch2

from

(select  
  obj$service.no_to_name(mest.no) xMest,  
  obj$service.no_to_name(lic.ext_element_no) XLicMest,  
  obj$service.no_to_name(lic.no) xLic,  
  mest1.description xDick,  
   case
   when case when 1=1 and 6=2 then 0  when 5465415=1 and 6=2 then 1 else 2 end = 5
     then 0
   else
     5
 end,
  (case  
    when --mest.no = 18878422  
      mest.code_fox = 155 and test = test
    then 'Азево-Салаушский участок с применением дифференцированной ставки по НДПИ'  
    when --mest.no = 18878596  
      mest.code_fox = 7 
    then 'Данково-Лебедянский горизонт Коробковского участка Бавлинского месторождения с применением дифференцированной ставки по НДПИ'  
    else ''  
    end) uch1,  
  (case  
    when --mest.NO = 18878422  
     mest.code_fox = 155 
     then 'Варзи-Омгинский участок'  
    when --mest.no = 18878596  
      mest.code_fox = 7 
    then 'Коробковский участок Бавлинского месторождения' 
      else ''  
   end) uch2,  
  (select --/*+rule*/      
         cha.from_months_start      
          from /*ssgdddjkddnjjd*/ omc$act_characteristics cha      
         where cha.characteristic_no = :tree_param1      
           and cha.mest_no = mest.no      
           and cha.license_no = lic.no      
           AND cha.FIELD_NO = cha.license_no     
           and cha.act_no = :pAct_no) xDobito,  /*

kdjgkajd
fajgakfj

fakdagkl
ajkdfjg*/
  (select --/*+rule*/      
         cha.from_months_start      
          from omc$act_characteristics cha      
         where cha.characteristic_no = :tree_param     
           and                                    cha.mest_no = mest.no      
           and cha.license_no = lic.no      
           AND cha.FIELD_NO = cha.license_no     
           and cha.act_no = :pAct_no) xPoteri,  
   (SELECT --/*+RULE*/        
                         from_months_start        
                    FROM omc$act_characteristics        
                   WHERE characteristic_no = :tree_param2        
                     AND mest_no = mest.NO        
                    -- AND license_no = l.NO        
                     AND FIELD_NO = license_no       
                     AND act_no = :pAct_no) xdobito2,    
   (SELECT --/*+RULE*/       
                         cha.from_months_start       
                    FROM omc$act_characteristics cha       
                   WHERE cha.characteristic_no = :tree_param3       
                   --  AND cha.license_no = l.NO       
                     AND cha.mest_no = mest.NO       
                     AND cha.FIELD_NO = cha.license_no    
                     AND cha.act_no = :pAct_no) xpoteri2,   
  tfc$point_service.get_value(tfc$point_service.resolve(:pStructure_No,      
                                                        to_char(lic.no) || ',' ||      
                                                        to_char(:pTFCChar),      
                                                        'F'),      
                                                        aliv.document_date) XNorm,  
  mest.code_fox xCode  
 from tatoil$ext_elements mest,      
      tatoil$ext_elements mest1,      
      tatoil$licenses lic,    
      (select document_date      
          from omc$act_of_leavings       
         where no = :pAct_no ) aliv  
 where  
(exists(select 1 ---/*+rule*/      
        ----  cha.from_months_start      
           from omc$act_characteristics cha      
          where cha.characteristic_no = :tree_param      
            and cha.license_no = lic.no      
            and cha.mest_no = mest.no     
            AND cha.FIELD_NO = cha.license_no     
            and cha.act_no = :pAct_no) ---- is not null 
 or  exists      
       (select 1 ---/*+rule*/      
         --- cha.from_year_start      
           from omc$act_characteristics cha      
          where cha.characteristic_no = :tree_param1     
            and cha.mest_no = mest.no      
            and cha.license_no = lic.no       
            AND cha.FIELD_NO = cha.license_no      
            and cha.act_no = :pAct_no ) ---is not null 
   ) and      
                  
       lic.ext_element_no = mest1.no  and  
       mest1.code_fox in ( 155, ----(Аз_Салауш)  
                           7 ) --- (Бавлинское) 
    --   mest1.NO  in ( 18878422, ----(Аз_Салауш)  
    --                  18878596 ) --- (Бавлинское) 
 ) f


