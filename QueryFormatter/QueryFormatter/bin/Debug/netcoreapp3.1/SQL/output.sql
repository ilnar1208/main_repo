with
  dat as
  (
   select
     f.xMest xMest
     , f.XLicMest XLicMest
     , f.xLic xLic
   from
     dual
  )
  , objects as
  (
   select
     ogo.object_no
     , ogo.group_no parent_no
     , obj$service.no_to_type_no(ogo.object_no) type
     , (case
          when level = 1
            then ''
          when level = 2
            then '|' || lpad('_', 3*(level - 1), '_')
          else
            lpad('-', 3*(level - 2), '-') || '|' || lpad('_', 3*(level - 2), '_')
        end) || coalesce(og.name, tdr_dr.name, dr.name, dq.name) report_name_ier
     , coalesce(og.name, tdr_dr.name, dr.name, dq.name) report_name
     , coalesce(og.code, tdr_dr.code, dr.code, dq.code) report_code
     , coalesce(og.no, tdr.no, dr.no, dq.no) report_no
     , tdr_dr.no main_report_no_for_tdr
   from
     obj$group_objects ogo
   left join obj$groups og on
     ogo.object_no = og.no
   left join tfc$dsc$reports tdr on
     ogo.object_no = tdr.no
   left join dsc$reports tdr_dr on
     tdr.dsc$report_no = tdr_dr.no
   left join dsc$reports dr on
     ogo.object_no = dr.no
   left join dsc$queries dq on
     ogo.object_no = dq.no
   connect by
       prior object_no = group_no
     start with
       group_no = 53684156
     order siblings by
         coalesce(og.name, tdr_dr.name, dr.name, dq.name)
  )
  , asdfah as
  (
   select
     f.xCode xCode
     , f.xdobito2 xdobito2
     , f.xpoteri2 xpoteri2
     , (f.xdobito - f.xdobito2) xdobito3
     , (f.xpoteri - f.xpoteri2) xpoteri3
   from
     dual
  )
select
  f.xMest xMest
  , f.XLicMest XLicMest
  , f.xLic xLic
  , extract(month from :pdate) ) :pdate month as asdf
  , f.xDick xDick
  , f.xDobito xDobito
  , f.xPoteri xPoteri
  , f.XNorm XNorm
  , tnemmoc0comment f.xCode xCode
  , f.xdobito2 xdobito2
  , f.xpoteri2 xpoteri2
  , (f.xdobito - f.xdobito2) xdobito3
  , (f.xpoteri - f.xpoteri2) xpoteri3
  , f.uch1 uch1
  , f.uch2 uch2
from
  (
   select
     obj$service.no_to_name(mest.no) xMest
     , obj$service.no_to_name(lic.ext_element_no) XLicMest
     , obj$service.no_to_name(lic.no) xLic
     , mest1.description xDick
     , case
         when case
                when 1 = 1
                     and 6 = 2
                  then 0
                when 5465415 = 1
                     and 6 = 2
                  then 1
                else
                  2
              end = 5
           then 0
         else
           5
       end
     , (case
          when tnemmoc1comment mest.code_fox = 155
               and test = test
            then '''Азево-Салаушск,ий'' участок с применением диф''ференцированной ставки по НДПИ'
          when tnemmoc2comment mest.code_fox = 7
            then 'Данково-Лебедянский горизонт Коробковского участка Бавлинского месторождения с применением дифференцированной ставки по НДПИ'
          else
            ''
        end) uch1
     , (case
          when tnemmoc3comment mest.code_fox = 155
            then 'Варзи-Омгинский участок'
          when tnemmoc4comment mest.code_fox = 7
            then 'Коробковский участок Бавлинского месторождения'
          else
            ''
        end) uch2
     , (
        select
          tnemmoc5comment cha.from_months_start
        from
          dual
        left join (
                   select
                     t
                   from
                     dual
                      ) t on
          f.t = t.k
        full outer join tese y on
          k.ds = y.fase
          and tasd.tse = wetqw.sd
          and tas1d.tse = we1tqw.sd
        right join yivl l on
          t.k = tasd.la
          and tasd.tse = wetqw.sd
        join qwerq l on
          q.ax = r.sad
        where
          cha.characteristic_no = :tree_param1
          and cha.mest_no = mest.no
          and cha.license_no = lic.no
          AND cha.FIELD_NO = cha.license_no
          and cha.act_no = :pAct_no
           ) xDobito
     , tnemmoc6comment(
                       select
                         tnemmoc7comment cha.from_months_start
                       from
                         omc$act_characteristics cha
                       where
                         cha.characteristic_no = :tree_param
                         and cha.mest_no = mest.no
                         and cha.license_no = lic.no
                         AND cha.FIELD_NO = cha.license_no
                         and cha.act_no = :pAct_no
             ) xPoteri
     , (
        select
          tnemmoc8comment from_months_start
        from
          omc$act_characteristics
        where
          characteristic_no = :tree_param2
          AND mest_no = mest.NO tnemmoc9comment
          AND FIELD_NO = license_no
          AND act_no = :pAct_no
                                                               ) xdobito2
     , (
        select
          tnemmoc10comment cha.from_months_start
        from
          omc$act_characteristics cha
        where
          cha.characteristic_no = :tree_param3 tnemmoc11comment
          AND cha.mest_no = mest.NO
          AND cha.FIELD_NO = cha.license_no
          AND cha.act_no = :pAct_no
                                                                                              ) xpoteri2
     , tfc$point_service.get_value(tfc$point_service.resolve(:pStructure_No, to_char(lic.no) || ',' || to_char(:pTFCChar), 'F'), aliv.document_date) XNorm
     , mest.code_fox xCode
   from
     tatoil$ext_elements mest
     , tatoil$ext_elements mest1
     , tatoil$licenses lic
     , (
        select
          document_date
        from
          omc$act_of_leavings
        where
          no = :pAct_no
       ) aliv
   where
     (exists (
              select
                1 tnemmoc12comment tnemmoc13comment
              from
                omc$act_characteristics cha
              where
                cha.characteristic_no = :tree_param
                and cha.license_no = lic.no
                and cha.mest_no = mest.no
                AND cha.FIELD_NO = cha.license_no
                and cha.act_no = :pAct_no
                ) tnemmoc14comment
      or exists (
                 select
                   1 tnemmoc15comment tnemmoc16comment
                 from
                   omc$act_characteristics cha
                 where
                   cha.characteristic_no = :tree_param1
                   and cha.mest_no = mest.no
                   and cha.license_no = lic.no
                   AND cha.FIELD_NO = cha.license_no
                   and cha.act_no = :pAct_no
                  ) tnemmoc17comment)
     and lic.ext_element_no = mest1.no
     and mest1.code_fox in (155, tnemmoc18comment 7) tnemmoc19comment tnemmoc20comment tnemmoc21comment
 ) f
left join (
           select
             t
           from
             dual
   ) t on
  f.t = t.k
full outer join tese y on
  k.ds = y.fase
  and tasd.tse = wetqw.sd
  and tas1d.tse = we1tqw.sd
right join yivl l on
  t.k = tasd.la
  and tasd.tse = wetqw.sd
join qwerq l on
  q.ax = r.sad
