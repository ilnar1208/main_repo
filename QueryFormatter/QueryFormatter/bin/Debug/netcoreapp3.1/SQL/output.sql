select
  NVL((
       select
         ee1.code_fox
       from
         tatoil$ext_elements ee1
       where
         ee1.NO = a5.domain_mest_no
      ), 0) mest_fox
  , (
     select
       ee1.NAME
     from
       tatoil$ext_elements ee1
     where
       ee1.NO = a5.domain_mest_no
    ) Мест_name
  , (
     select
       eer.NAME
     from
       tatoil$ext_elements eer
       , DICTIONARY d
     where
       eer.NO = a5.domain_plo_no
       AND d.NO = eer.type_no
       AND (d.upper_code = 1
            OR d.upper_code = 2)
    ) n_plo
  , (
     select
       tatoil$organization_service.no_to_number_name(a5.ds_no) fox_ceh
     from
       DUAL
    ) ceh
  , (
     select
       oe.ИМЯ
     from
       organizations oe
     where
       oe.NO = a5.dorg_no
    ) ngdu
  , (
     select
       oe.ОБОЗНАЧЕНИЕ
     from
       organizations oe
     where
       oe.NO = a5.dorg_no
    ) ngdu_k
  , (
     select
       obj$link_other_service.no_to_id(a5.dorg_no, ' fox ') f
     from
       dual
    ) ngdu_fox
  , a5.*
  , (CASE
       WHEN a5.sost = - 999
         THEN ' ЦДНГ '
       ELSE (
             select
               obj$service.no_to_name(a5.sost) s
             from
               DUAL
            )
     END) sost_n
  , (case
       when a5.zak_tek < = 0
            and a5.sost < > - 999
         then (
               select
                 dic.name
               from
                 omc$faults f1fs1
                 , OMC$FAULT_REASONS f1fs2
                 , DICTIONARY dic
               where
                 f1fs1.object_no = a5.w_no
                 AND f1fs1.FAULT_DATE < ADD_MONTHS(:dt_pdate, 1)
                 AND f1fs1.REPAIR_DATE > ADD_MONTHS(:dt_pdate, 1)
                 AND f1fs2.fault_no = f1fs1.no
                 AND f1fs2.ENTRY_DATE > = f1fs1.FAULT_DATE
                 AND f1fs2.end_date < = f1fs1.REPAIR_DATE
                 AND f1fs2.ENTRY_DATE < ADD_MONTHS(:dt_pdate, 1)
                 AND f1fs2.end_date > ADD_MONTHS(:dt_pdate, 1)
                 and f1fs2.ENTRY_DATE = (
                                         select
                                           max(f1fs0.ENTRY_DATE)
                                         from
                                           omc$faults f1f0
                                           , OMC$FAULT_REASONS f1fs0
                                         where
                                           f1f0.object_no = a5.w_no
                                           AND f1f0.FAULT_DATE < ADD_MONTHS(:dt_pdate, 1)
                                           AND f1f0.REPAIR_DATE > ADD_MONTHS(:dt_pdate, 1)
                                           AND f1fs0.fault_no = f1f0.no
                                           AND f1fs0.ENTRY_DATE > = f1f0.FAULT_DATE
                                           AND f1fs0.end_date < = f1f0.REPAIR_DATE
                                           AND f1fs0.ENTRY_DATE < ADD_MONTHS(:dt_pdate, 1)
                                           AND f1fs0.end_date > ADD_MONTHS(:dt_pdate, 1)
                                        )
                 and dic.no = f1fs2.reason_no
                 and rownum < 2
              )
       else
         ' '
     end) prost
  , :dt_pdate_end d_end
from
  (
   select
     a4.report_name
     , a4.w_no
     , a4.dorg_no
     , a4.domain_plo_no
     , a4.domain_mest_no
     , a4.sost
     , MAX(a4.zak_tek) zak_tek
     , MIN(a4.date_zak) date_zak
     , SUM(a4.zak) zak
     , MAX(a4.p1) p1
     , MAX(a4.t1) t1
     , MAX(a4.p2) p2
     , MAX(a4.t2) t2
     , MAX(a4.p3) p3
     , MAX(a4.t3) t3
     , MAX(a4.p4) p4
     , MAX(a4.t4) t4
     , MAX(a4.p5) p5
     , MAX(a4.t5) t5
     , MAX(a4.p6) p6
     , MAX(a4.t6) t6
     , MAX(a4.p7) p7
     , MAX(a4.t7) t7
     , MAX(a4.p8) p8
     , MAX(a4.t8) t8
     , MAX(a4.p9) p9
     , MAX(a4.t9) t9
     , MAX(a4.p10) p10
     , MAX(a4.t10) t10
     , MAX(a4.p11) p11
     , MAX(a4.t11) t11
     , MAX(a4.p12) p12
     , MAX(a4.t12) t12
     , (
        select
          MIN(org.ds_no)
        from
          tatoil$well_geologyes wg
          , well_descriptions wd
          , (
             select
               oe.NO org_no
               , oe.domain_shop_no ds_no
             from
               organizations oe
             where
               oe.type_no = :dt_poe3
               OR oe.type_no = :dt_poe19
            ) org
        where
          wg.well_no = a4.w_no
          AND wg.type_object = ' NEW '
          AND wg.date_begin < ADD_MONTHS(:dt_pdate, 1)
          AND wg.date_end > TRUNC(:dt_pdate, ' YYYY ')
          AND wg.date_begin = (
                               select
                                 MAX(wg1.date_begin)
                               from
                                 tatoil$well_geologyes wg1
                                 , well_descriptions wd1
                                 , (
                                    select
                                      oe.NO org_no
                                      , oe.domain_shop_no ds_no
                                    from
                                      organizations oe
                                    where
                                      oe.type_no = :dt_poe3
                                      OR oe.type_no = :dt_poe19
                                   ) org1
                                 , tatoil$geo_work_objects obr1
                                 , tatoil$ext_elements ee1
                               where
                                 wg1.well_no = wg.well_no
                                 AND wg1.type_object = ' NEW '
                                 AND wg1.date_begin < ADD_MONTHS(:dt_pdate, 1)
                                 AND wg1.date_end > TRUNC(:dt_pdate, ' YYYY ')
                                 AND wd1.geology_no = NVL(wg1.parent_geology_no, wg1.NO)
                                 AND wd1.organization_no = org1.org_no
                                 AND wd1.begin_date > = wg1.date_begin
                                 AND wd1.end_date < = wg1.date_end
                                 AND wd1.begin_date < ADD_MONTHS(:dt_pdate, 1)
                                 AND wd1.end_date > TRUNC(:dt_pdate, ' YYYY ')
                                 AND obr1.NO = wg1.object_no
                                 AND ee1.NO = obr1.ext_element_no
                                 AND (a4.domain_plo_no IS NULL
                                      OR ee1.domain_plo_no = a4.domain_plo_no)
                                 AND ee1.domain_mest_no = a4.domain_mest_no
                              )
          AND wd.geology_no = NVL(wg.parent_geology_no, wg.NO)
          AND wd.organization_no = org.org_no
          AND wd.begin_date < wg.date_end
          AND wd.end_date > wg.date_begin
          AND wd.begin_date = (
                               select
                                 MAX(wd1.begin_date)
                               from
                                 tatoil$well_geologyes wg1
                                 , well_descriptions wd1
                                 , (
                                    select
                                      oe.NO org_no
                                      , oe.domain_shop_no ds_no
                                    from
                                      organizations oe
                                    where
                                      oe.type_no = :dt_poe3
                                      OR oe.type_no = :dt_poe19
                                   ) org1
                                 , tatoil$geo_work_objects obr1
                                 , tatoil$ext_elements ee1
                               where
                                 wg1.well_no = wg.well_no
                                 AND wg1.type_object = ' NEW '
                                 AND wg1.date_begin < ADD_MONTHS(:dt_pdate, 1)
                                 AND wg1.date_end > TRUNC(:dt_pdate, ' YYYY ')
                                 AND wd1.geology_no = NVL(wg1.parent_geology_no, wg1.NO)
                                 AND wd1.organization_no = org1.org_no
                                 AND wd1.begin_date > = wg1.date_begin
                                 AND wd1.end_date < = wg1.date_end
                                 AND wd1.begin_date < ADD_MONTHS(:dt_pdate, 1)
                                 AND wd1.end_date > TRUNC(:dt_pdate, ' YYYY ')
                                 AND obr1.NO = wg1.object_no
                                 AND ee1.NO = obr1.ext_element_no
                                 AND (a4.domain_plo_no IS NULL
                                      OR ee1.domain_plo_no = a4.domain_plo_no)
                                 AND ee1.domain_mest_no = a4.domain_mest_no
                              )
       ) ds_no
   from
     (
      select
        a3.report_name
        , a3.w_no
        , a3.wg_object_no
        , a3.zak
        , a3.v_mes
        , a3.dorg_no
        , a3.ds_no
        , a3.domain_plo_no
        , a3.domain_mest_no
        , a3.sost
        , a3.date_zak
        , (CASE
             WHEN a3.v_mes = TO_NUMBER(TO_CHAR(:dt_pdate, ' mm '))
               THEN a3.zak
             ELSE
               0
           END) zak_tek
        , (CASE
             WHEN a3.v_mes = 1
               THEN a3.priem
             ELSE
               0
           END) p1
        , (CASE
             WHEN a3.v_mes = 1
               THEN t3.t_eks
             ELSE
               0
           END) t1
        , (CASE
             WHEN a3.v_mes = 2
               THEN a3.priem
             ELSE
               0
           END) p2
        , (CASE
             WHEN a3.v_mes = 2
               THEN t3.t_eks
             ELSE
               0
           END) t2
        , (CASE
             WHEN a3.v_mes = 3
               THEN a3.priem
             ELSE
               0
           END) p3
        , (CASE
             WHEN a3.v_mes = 3
               THEN t3.t_eks
             ELSE
               0
           END) t3
        , (CASE
             WHEN a3.v_mes = 4
               THEN a3.priem
             ELSE
               0
           END) p4
        , (CASE
             WHEN a3.v_mes = 4
               THEN t3.t_eks
             ELSE
               0
           END) t4
        , (CASE
             WHEN a3.v_mes = 5
               THEN a3.priem
             ELSE
               0
           END) p5
        , (CASE
             WHEN a3.v_mes = 5
               THEN t3.t_eks
             ELSE
               0
           END) t5
        , (CASE
             WHEN a3.v_mes = 6
               THEN a3.priem
             ELSE
               0
           END) p6
        , (CASE
             WHEN a3.v_mes = 6
               THEN t3.t_eks
             ELSE
               0
           END) t6
        , (CASE
             WHEN a3.v_mes = 7
               THEN a3.priem
             ELSE
               0
           END) p7
        , (CASE
             WHEN a3.v_mes = 7
               THEN t3.t_eks
             ELSE
               0
           END) t7
        , (CASE
             WHEN a3.v_mes = 8
               THEN a3.priem
             ELSE
               0
           END) p8
        , (CASE
             WHEN a3.v_mes = 8
               THEN t3.t_eks
             ELSE
               0
           END) t8
        , (CASE
             WHEN a3.v_mes = 9
               THEN a3.priem
             ELSE
               0
           END) p9
        , (CASE
             WHEN a3.v_mes = 9
               THEN t3.t_eks
             ELSE
               0
           END) t9
        , (CASE
             WHEN a3.v_mes = 10
               THEN a3.priem
             ELSE
               0
           END) p10
        , (CASE
             WHEN a3.v_mes = 10
               THEN t3.t_eks
             ELSE
               0
           END) t10
        , (CASE
             WHEN a3.v_mes = 11
               THEN a3.priem
             ELSE
               0
           END) p11
        , (CASE
             WHEN a3.v_mes = 11
               THEN t3.t_eks
             ELSE
               0
           END) t11
        , (CASE
             WHEN a3.v_mes = 12
               THEN a3.priem
             ELSE
               0
           END) p12
        , (CASE
             WHEN a3.v_mes = 12
               THEN t3.t_eks
             ELSE
               0
           END) t12
      from
        (
         select
           a2.report_name
           , a2.w_no
           , a2.wg_object_no
           , a2.v_mes
           , a2.dorg_no
           , a2.ds_no
           , ee.domain_plo_no
           , ee.domain_mest_no
           , SUM(a2.zak) zak
           , NVL((TO_CHAR((
                           select
                             MIN(v00.value_date)
                           from
                             tfc$points p00
                             , tfc$point_values v00
                             , (
                                select
                                  NO no1
                                from
                                  DICTIONARY da
                                where
                                  da.NO = :dt_p_ag34
                                  OR da.NO = :dt_p_ag35
                                  OR da.NO = :dt_p_ag45
                                  OR da.NO = :dt_p_ag46
                                  OR da.NO = :dt_p_ag47
                                  OR da.NO = :dt_p_ag22
                                  OR da.NO = :dt_p_ag48
                                  OR da.NO = :dt_p_ag84
                                  OR da.NO = :dt_p_ag69
                                  OR da.NO = :dt_p_ag1
                               ) ag_no2
                             , tatoil$well_geologyes wg_dw
                             , well_descriptions wd_dw
                             , (
                                select
                                  oe.NO org_no
                                  , oe.domain_shop_no ds_no
                                  , oe.domain_org_no dorg_no
                                from
                                  organizations oe
                                where
                                  (oe.domain_org_no = :dt_porg
                                   or oe.DOMAIN_COMPANY_NO = :dt_porg)
                                  AND (oe.type_no = :dt_poe3
                                       OR oe.type_no = :dt_poe19)
                               ) org_dw
                           where
                             wg_dw.well_no = a2.w_no
                             and wg_dw.type_object = ' NEW '
                             AND wg_dw.date_begin < ADD_MONTHS(:dt_pdate, 1)
                             AND wg_dw.date_end > to_date(to_char(TRUNC(:dt_pdate, ' YYYY '), ' dd.mm.yyyy '), ' dd.mm.yyyy ')
                             AND wd_dw.geology_no = NVL(wg_dw.parent_geology_no, wg_dw.NO)
                             AND wd_dw.organization_no = org_dw.org_no
                             AND wd_dw.begin_date < wg_dw.date_end
                             AND wd_dw.end_date > wg_dw.date_begin
                             AND wd_dw.begin_date < ADD_MONTHS(:dt_pdate, 1)
                             AND wd_dw.end_date > to_date(to_char(TRUNC(:dt_pdate, ' YYYY '), ' dd.mm.yyyy '), ' dd.mm.yyyy ')
                             AND wd_dw.state_no < > :dt_pstate1
                             AND wd_dw.state_no < > :dt_pstate2
                             and wd_dw.purpose_no = :Dt_p_nazn2
                             and p00.structure_no = :dt_pstru_mera
                             AND (p00.objects_list = TO_CHAR(a2.w_no) || ', ' || TO_CHAR(:dt_p1003) || ', ' || TO_CHAR(wg_dw.object_no) || ', ' || TO_CHAR(ag_no2.no1)
                                  OR p00.objects_list = TO_CHAR(a2.w_no) || ', ' || TO_CHAR(:Dt_p592) || ', ' || TO_CHAR(wg_dw.object_no) || ', ' || TO_CHAR(ag_no2.no1)
                                  OR p00.objects_list = TO_CHAR(a2.w_no) || ', ' || TO_CHAR(:Dt_p593) || ', ' || TO_CHAR(wg_dw.object_no) || ', ' || TO_CHAR(ag_no2.no1))
                             AND v00.point_no = p00.NO
                             and v00.value_date > = wd_dw.begin_date
                             and v00.value_date < wd_dw.end_date
                          ), ' dd.mm.yyyy ')), ' ') date_zak
           , ROUND(MAX(a2.priem), 2) priem
           , (CASE
                WHEN (
                      select
                        org_m.КРАТКОЕ_СТРУКТУРНОЕ_ИМЯ
                      from
                        tatoil$well_geologyes wg
                        , well_descriptions wd
                        , (
                           select
                             oe.NO org_no
                             , oe.КРАТКОЕ_СТРУКТУРНОЕ_ИМЯ
                           from
                             organizations oe
                          ) org_m
                      where
                        wg.well_no = a2.w_no
                        AND wg.type_object = ' NEW '
                        AND wg.object_no = a2.wg_object_no
                        AND wg.date_begin = (
                                             select
                                               MAX(wg1.date_begin)
                                             from
                                               tatoil$well_geologyes wg1
                                             where
                                               wg1.well_no = a2.w_no
                                               AND wg1.type_object = ' NEW '
                                               AND wg1.object_no = a2.wg_object_no
                                               AND wg1.date_begin < ADD_MONTHS(:dt__pdate, 1)
                                            )
                        AND wd.geology_no = NVL(wg.parent_geology_no, wg.NO)
                        AND wd.begin_date = (
                                             select
                                               MAX(wd1.begin_date)
                                             from
                                               well_descriptions wd1
                                             where
                                               wd1.geology_no = NVL(wg.parent_geology_no, wg.NO)
                                               AND wd1.begin_date > = wg.date_begin
                                               AND wd1.end_date < = wg.date_end
                                               AND wd1.begin_date < ADD_MONTHS(:dt_pdate, 1)
                                            )
                        AND org_m.org_no = wd.organization_no
                     ) LIKE ' %ЦДНГ% '
                  THEN - 999
                ELSE
                  NVL((
                       select
                         v0.object_no_value
                       from
                         tfc$point_values v0
                         , tfc$points p0
                       where
                         p0.structure_no = :dt_pstru_skw
                         AND p0.objects_list = TO_CHAR(a2.w_no) || '
                         , ' || TO_CHAR(:dt_p_r_sost) || '
                         , ' || TO_CHAR(a2.wg_object_no)
                         AND v0.point_no = p0.NO
                         AND v0.value_date > TRUNC(:dt_pdate, ' YYYY ')
                         AND v0.value_date < = ADD_MONTHS(:dt_pdate, 1)
                         AND v0.value_date = (
                                              select
                                                MAX(tpv.value_date)
                                              from
                                                tfc$point_values tpv
                                                , DICTIONARY dic
                                              where
                                                tpv.point_no = v0.point_no
                                                AND tpv.value_date < = ADD_MONTHS(:dt_pdate, 1)
                                                AND tpv.value_date > TRUNC(:dt_pdate, ' YYYY ')
                                                AND dic.NO = tpv.object_no_value
                                                AND dic.code < = 29
                                                AND dic.code > = 21
                                             )
                         AND ROWNUM < 2
                      ), (
                          select
                            v0.object_no_value
                          from
                            tatoil$well_geologyes wg3
                            , tfc$point_values v0
                            , tfc$points p0
                            , DICTIONARY dic
                          where
                            wg3.well_no = a2.w_no
                            AND wg3.type_object = ' NEW '
                            AND wg3.date_begin < ADD_MONTHS(:dt_pdate, 1)
                            AND wg3.date_end > TRUNC(:dt_pdate, ' YYYY ')
                            AND wg3.is_main_fond = ' T '
                            AND p0.structure_no = :dt_pstru_skw
                            AND p0.objects_list = TO_CHAR(wg3.well_no) || '
                            , ' || TO_CHAR(:dt_p_r_sost) || '
                            , ' || TO_CHAR(wg3.object_no)
                            AND v0.point_no = p0.NO
                            AND v0.value_date > TRUNC(:dt_pdate, ' YYYY ')
                            AND v0.value_date < = ADD_MONTHS(:dt_pdate, 1)
                            AND v0.value_date = (
                                                 select
                                                   MAX(tpv.value_date)
                                                 from
                                                   tfc$point_values tpv
                                                   , well_descriptions wd3
                                                   , (
                                                      select
                                                        oe.NO org_no
                                                      from
                                                        organizations oe
                                                      where
                                                        oe.type_no = :dt_poe3
                                                        OR oe.type_no = :dt_poe19
                                                     ) org3
                                                   , DICTIONARY dic
                                                 where
                                                   wd3.geology_no = NVL(wg3.parent_geology_no, wg3.NO)
                                                   AND wd3.organization_no = org3.org_no
                                                   AND wd3.begin_date > = wg3.date_begin
                                                   AND wd3.end_date < = wg3.date_end
                                                   AND wd3.begin_date < ADD_MONTHS(:dt_pdate, 1)
                                                   AND wd3.end_date > TRUNC(:dt_pdate, ' YYYY ')
                                                   AND tpv.point_no = v0.point_no
                                                   AND tpv.value_date < = wd3.end_date
                                                   AND tpv.value_date > wd3.begin_date
                                                   AND tpv.value_date < = ADD_MONTHS(:dt_pdate, 1)
                                                   AND tpv.value_date > TRUNC(:dt_pdate, ' YYYY ')
                                                   AND dic.NO = tpv.object_no_value
                                                   AND dic.code < = 29
                                                   AND dic.code > = 21
                                                )
                            AND dic.NO = v0.object_no_value
                            AND dic.code < = 29
                            AND dic.code > = 21
                            AND ROWNUM < 2
                         ))
              END) sost
         from
           (
            select
              a1.report_name
              , a1.w_no
              , a1.wg_object_no
              , TO_NUMBER(TO_CHAR(TRUNC(a1.value_date, ' mm '), ' mm ')) v_mes
              , a1.value_date
              , a1.dorg_no
              , a1.ds_no
              , NVL((
                     select
                       v3.VALUE
                     from
                       tfc$point_values v3
                       , tfc$points p3
                       , (
                          select
                            NO no1
                          from
                            DICTIONARY da
                          where
                            da.NO = :dt_p_ag34
                            OR da.NO = :dt_p_ag35
                            OR da.NO = :dt_p_ag45
                            OR da.NO = :dt_p_ag46
                            OR da.NO = :dt_p_ag47
                            OR da.NO = :dt_p_ag22
                            OR da.NO = :dt_p_ag48
                            OR da.NO = :dt_p_ag84
                            OR da.NO = :dt_p_ag69
                            OR da.NO = :dt_p_ag1
                            OR da.NO = :dt_p_ag0
                         ) ag_no2
                     where
                       p3.structure_no = :dt_pstru_mera
                       AND p3.objects_list = TO_CHAR(a1.w_no) || '
                       , ' || TO_CHAR(:dt_p1003) || '
                       , ' || TO_CHAR(a1.wg_object_no) || '
                       , ' || TO_CHAR(ag_no2.no1)
                       AND v3.point_no = p3.NO
                       AND v3.value_date = a1.value_date
                       AND v3.VALUE > 0
                    ), 0) zak
              , NVL((
                     select
                       v3.VALUE
                     from
                       tfc$points p3
                       , tfc$point_values v3
                       , (
                          select
                            NO no1
                          from
                            DICTIONARY da
                          where
                            da.NO = :dt_p_ag34
                            OR da.NO = :dt_p_ag35
                            OR da.NO = :dt_p_ag45
                            OR da.NO = :dt_p_ag46
                            OR da.NO = :dt_p_ag47
                            OR da.NO = :dt_p_ag22
                            OR da.NO = :dt_p_ag48
                            OR da.NO = :dt_p_ag84
                            OR da.NO = :dt_p_ag69
                            OR da.NO = :dt_p_ag1
                         ) ag_no2
                     where
                       p3.structure_no = :dt_pstru_mera
                       AND p3.objects_list = TO_CHAR(a1.w_no) || '
                       , ' || TO_CHAR(:dt_p32) || '
                       , ' || TO_CHAR(a1.wg_object_no) || '
                       , ' || TO_CHAR(ag_no2.no1)
                       AND v3.point_no = p3.NO
                       AND v3.value_date = a1.value_date
                       AND v3.VALUE > 0
                    ), 0) priem
            from
              (
               select
                 DISTINCT a.report_name
                 , a.w_no
                 , a.wg_object_no
                 , a.wd_begin_date
                 , a.wd_end_date
                 , v.value_date
                 , a.ds_no
                 , a.dorg_no
               from
                 (
                  select
                    w.report_name
                    , wg.well_no w_no
                    , wg.object_no wg_object_no
                    , wd.begin_date wd_begin_date
                    , wd.end_date wd_end_date
                    , org.ds_no
                    , org.dorg_no
                  from
                    wells w
                    , tatoil$well_geologyes wg
                    , well_descriptions wd
                    , (
                       select
                         oe.NO org_no
                         , oe.domain_shop_no ds_no
                         , oe.domain_org_no dorg_no
                       from
                         organizations oe
                       where
                         (oe.domain_org_no = :dt_porg
                          or oe.DOMAIN_COMPANY_NO = :dt_porg)
                         AND (oe.type_no = :dt_poe3
                              OR oe.type_no = :dt_poe19)
                      ) org
                  where
                    wg.well_no = w.NO
                    AND wg.type_object = ' NEW '
                    AND wg.date_begin < ADD_MONTHS(:dt_pdate, 1)
                    AND wg.date_end > TRUNC(:dt_pdate, ' YYYY ')
                    AND wd.geology_no = NVL(wg.parent_geology_no, wg.NO)
                    AND wd.organization_no = org.org_no
                    AND wd.begin_date < wg.date_end
                    AND wd.end_date > wg.date_begin
                    AND wd.begin_date < ADD_MONTHS(:dt_pdate, 1)
                    AND wd.end_date > TRUNC(:dt_pdate, ' YYYY ')
                    AND wd.state_no < > :dt_pstate1
                    AND wd.state_no < > :dt_pstate2
                    AND wd.purpose_no < > :dt_p_nazn14
                    AND (EXISTS (
                                 select
                                   1
                                 from
                                   tfc$point_values v0
                                   , tfc$points p0
                                   , DICTIONARY dic
                                 where
                                   p0.structure_no = :dt_pstru_skw
                                   AND p0.objects_list = TO_CHAR(wg.well_no) || '
                                   , ' || TO_CHAR(:dt_p_r_sost) || '
                                   , ' || TO_CHAR(wg.object_no)
                                   AND v0.point_no = p0.NO
                                   AND v0.value_date > TRUNC(:dt_pdate, ' YYYY ')
                                   AND v0.value_date < = ADD_MONTHS(:dt_pdate, 1)
                                   AND v0.value_date > wd.begin_date
                                   AND v0.value_date < = wd.end_date
                                   AND dic.NO = v0.object_no_value
                                   AND dic.code < = 29
                                   AND dic.code > = 21
                                   AND ROWNUM < 2
                                )
                         OR EXISTS (
                                    select
                                      1
                                    from
                                      tatoil$well_geologyes wg3
                                      , well_descriptions wd3
                                      , (
                                         select
                                           oe.NO org_no
                                           , oe.domain_shop_no ds_no
                                         from
                                           organizations oe
                                         where
                                           oe.type_no = :dt_poe3
                                           OR oe.type_no = :dt_poe19
                                        ) org3
                                      , tfc$point_values v0
                                      , tfc$points p0
                                      , DICTIONARY dic
                                    where
                                      wg3.well_no = wg.well_no
                                      AND wg3.type_object = ' NEW '
                                      AND wg3.date_begin < ADD_MONTHS(:dt_pdate, 1)
                                      AND wg3.date_end > TRUNC(:dt_pdate, ' YYYY ')
                                      AND wg3.date_begin < wg.date_end
                                      AND wg3.date_end > wg.date_begin
                                      AND wg3.is_main_fond = ' T '
                                      AND wd3.geology_no = NVL(wg3.parent_geology_no, wg3.NO)
                                      AND wd3.organization_no = org3.org_no
                                      AND wd3.begin_date < wg3.date_end
                                      AND wd3.end_date > wg3.date_begin
                                      AND wd3.begin_date < ADD_MONTHS(:dt_pdate, 1)
                                      AND wd3.end_date > TRUNC(:dt_pdate, ' YYYY ')
                                      AND p0.structure_no = :dt_pstru_skw
                                      AND p0.objects_list = TO_CHAR(wg3.well_no) || '
                                      , ' || TO_CHAR(:dt_p_r_sost) || '
                                      , ' || TO_CHAR(wg3.object_no)
                                      AND v0.point_no = p0.NO
                                      AND v0.value_date > TRUNC(:dt_pdate, ' YYYY ')
                                      AND v0.value_date < = ADD_MONTHS(:dt_pdate, 1)
                                      AND v0.value_date > wd3.begin_date
                                      AND v0.value_date < = wd3.end_date
                                      AND dic.NO = v0.object_no_value
                                      AND dic.code < = 29
                                      AND dic.code > = 21
                                      AND ROWNUM < 2
                                   ))
                 ) a
                 , tfc$point_values v
                 , tfc$points p
                 , (
                    select
                      NO no1
                    from
                      DICTIONARY da
                    where
                      da.NO = :dt_p_ag34
                      OR da.NO = :dt_p_ag35
                      OR da.NO = :dt_p_ag45
                      OR da.NO = :dt_p_ag46
                      OR da.NO = :dt_p_ag47
                      OR da.NO = :dt_p_ag22
                      OR da.NO = :dt_p_ag48
                      OR da.NO = :dt_p_ag84
                      OR da.NO = :dt_p_ag69
                      OR da.NO = :dt_p_ag1
                      OR da.NO = :dt_p_ag0
                   ) ag_no
                 , (
                    select
                      NO
                    from
                      tfc$characteristics c2
                    where
                      c2.upper_code = ' 592 '
                      OR c2.upper_code = ' 593 '
                      OR c2.upper_code = ' 594 '
                      OR c2.upper_code = ' 591 '
                      OR c2.upper_code = ' 625 '
                   ) tc
               where
                 p.structure_no = :dt_pstru_mera
                 AND p.objects_list = TO_CHAR(a.w_no) || '
                 , ' || TO_CHAR(tc.NO) || '
                 , ' || TO_CHAR(a.wg_object_no) || '
                 , ' || TO_CHAR(ag_no.no1)
                 AND v.point_no = p.NO
                 AND v.value_date > = a.wd_begin_date
                 AND v.value_date < a.wd_end_date
                 AND v.value_date > = TRUNC(:dt_pdate, ' YYYY ')
                 AND v.value_date < ADD_MONTHS(:dt_pdate, 1)
                 AND v.VALUE > 0
                 AND COALESCE((
                               select
                                 1
                               from
                                 tfc$point_values v0
                                 , tfc$points p0
                                 , DICTIONARY dic
                               where
                                 p0.structure_no = :dt_pstru_skw
                                 AND p0.objects_list = TO_CHAR(a.w_no) || '
                                 , ' || TO_CHAR(:dt_p_r_sost) || '
                                 , ' || TO_CHAR(a.wg_object_no)
                                 AND v0.point_no = p0.NO
                                 AND v0.value_date > TRUNC(:dt_pdate, ' YYYY ')
                                 AND v0.value_date < = ADD_MONTHS(:dt_pdate, 1)
                                 AND v0.value_date > a.wd_begin_date
                                 AND v0.value_date < = a.wd_end_date
                                 AND v0.value_date = (
                                                      select
                                                        MIN(tpv.value_date)
                                                      from
                                                        tfc$point_values tpv
                                                      where
                                                        tpv.point_no = v0.point_no
                                                        AND tpv.value_date < = a.wd_end_date
                                                        AND tpv.value_date > a.wd_begin_date
                                                        AND tpv.value_date > v.value_date
                                                        AND tpv.value_date < = ADD_MONTHS(v.value_date, 1)
                                                     )
                                 AND dic.NO = v0.object_no_value
                                 AND dic.code < = 29
                                 AND dic.code > = 21
                                 AND ROWNUM < 2
                              ), (
                                  select
                                    1
                                  from
                                    tatoil$well_geologyes wg3
                                    , well_descriptions wd3
                                    , (
                                       select
                                         oe.NO org_no
                                       from
                                         organizations oe
                                       where
                                         oe.type_no = :dt_poe3
                                         OR oe.type_no = :dt_poe19
                                      ) org3
                                    , tfc$point_values v0
                                    , tfc$points p0
                                    , DICTIONARY dic
                                  where
                                    wg3.well_no = a.w_no
                                    AND wg3.type_object = ' NEW '
                                    AND wg3.date_begin < ADD_MONTHS(:dt_pdate, 1)
                                    AND wg3.date_end > TRUNC(:dt_pdate, ' YYYY ')
                                    AND wg3.is_main_fond = ' T '
                                    AND wd3.geology_no = NVL(wg3.parent_geology_no, wg3.NO)
                                    AND wd3.organization_no = org3.org_no
                                    AND wd3.begin_date < wg3.date_end
                                    AND wd3.end_date > wg3.date_begin
                                    AND wd3.begin_date < ADD_MONTHS(:dt_pdate, 1)
                                    AND wd3.end_date > TRUNC(:dt_pdate, ' YYYY ')
                                    AND p0.structure_no = :dt_pstru_skw
                                    AND p0.objects_list = TO_CHAR(wg3.well_no) || '
                                    , ' || TO_CHAR(:dt_p_r_sost) || '
                                    , ' || TO_CHAR(wg3.object_no)
                                    AND v0.point_no = p0.NO
                                    AND v0.value_date > TRUNC(:dt_pdate, ' YYYY ')
                                    AND v0.value_date < = ADD_MONTHS(:dt_pdate, 1)
                                    AND v0.value_date > wd3.begin_date
                                    AND v0.value_date < = wd3.end_date
                                    AND v0.value_date = (
                                                         select
                                                           MIN(tpv.value_date)
                                                         from
                                                           tfc$point_values tpv
                                                         where
                                                           tpv.point_no = v0.point_no
                                                           AND tpv.value_date < = wd3.end_date
                                                           AND tpv.value_date > wd3.begin_date
                                                           AND tpv.value_date > v.value_date
                                                           AND tpv.value_date < = ADD_MONTHS(v.value_date, 1)
                                                        )
                                    AND dic.NO = v0.object_no_value
                                    AND dic.code < = 29
                                    AND dic.code > = 21
                                    AND ROWNUM < 2
                                 ), 0) > 0
              ) a1
           ) a2
           , tatoil$geo_work_objects obr
           , tatoil$ext_elements ee
         where
           obr.NO = a2.wg_object_no
           AND ee.NO = obr.ext_element_no
         GROUP by
           a2.report_name
           , a2.w_no
           , a2.wg_object_no
           , a2.v_mes
           , a2.dorg_no
           , a2.ds_no
           , ee.domain_plo_no
           , ee.domain_mest_no
        ) a3
        , (
           select
             a3_t.w_no
             , a3_t.domain_plo_no
             , a3_t.domain_mest_no
             , a3_t.sost_t
             , a3_t.v_mes
             , MAX(t_eks) t_eks
           from
             (
              select
                a2_t.w_no
                , a2_t.wg_object_no
                , a2_t.domain_plo_no
                , a2_t.domain_mest_no
                , a2_t.sost_t
                , a2_t.v_mes
                , SUM(t_eks) t_eks
              from
                (
                 select
                   a1_t.w_no
                   , a1_t.wg_object_no
                   , a1_t.value_date
                   , TO_NUMBER(TO_CHAR(TRUNC(a1_t.value_date, ' mm '), ' mm ')) v_mes
                   , NVL((
                          select
                            SUM(v3.VALUE)
                          from
                            tfc$point_values v3
                            , tfc$points p3
                            , (
                               select
                                 NO no1
                               from
                                 DICTIONARY da
                               where
                                 da.NO = :dt_p_ag34
                                 OR da.NO = :dt_p_ag35
                                 OR da.NO = :dt_p_ag45
                                 OR da.NO = :dt_p_ag46
                                 OR da.NO = :dt_p_ag47
                                 OR da.NO = :dt_p_ag22
                                 OR da.NO = :dt_p_ag48
                                 OR da.NO = :dt_p_ag84
                                 OR da.NO = :dt_p_ag69
                                 OR da.NO = :dt_p_ag1
                                 OR da.NO = :dt_p_ag0
                              ) ag_no2
                            , (
                               select
                                 NO
                               from
                                 tfc$characteristics c2
                               where
                                 c2.upper_code = ' 593 '
                              ) tc1
                          where
                            p3.structure_no = :dt_pstru_mera
                            AND p3.objects_list = TO_CHAR(a1_t.w_no) || '
                            , ' || TO_CHAR(tc1.NO) || '
                            , ' || TO_CHAR(a1_t.wg_object_no) || '
                            , ' || TO_CHAR(ag_no2.no1)
                            AND v3.point_no = p3.NO
                            AND v3.value_date = a1_t.value_date
                            AND v3.VALUE > 0
                         ), 0) t_eks
                   , ee.domain_plo_no
                   , ee.domain_mest_no
                   , (CASE
                        WHEN (
                              select
                                org_m.КРАТКОЕ_СТРУКТУРНОЕ_ИМЯ
                              from
                                tatoil$well_geologyes wg
                                , well_descriptions wd
                                , (
                                   select
                                     oe.NO org_no
                                     , oe.КРАТКОЕ_СТРУКТУРНОЕ_ИМЯ
                                   from
                                     organizations oe
                                  ) org_m
                              where
                                wg.well_no = a1_t.w_no
                                AND wg.type_object = ' NEW '
                                AND wg.object_no = a1_t.wg_object_no
                                AND wg.date_begin = (
                                                     select
                                                       MAX(wg1.date_begin)
                                                     from
                                                       tatoil$well_geologyes wg1
                                                     where
                                                       wg1.well_no = a1_t.w_no
                                                       AND wg1.type_object = ' NEW '
                                                       AND wg1.object_no = a1_t.wg_object_no
                                                       AND wg1.date_begin < ADD_MONTHS(:dt_pdate, 1)
                                                    )
                                AND wd.geology_no = NVL(wg.parent_geology_no, wg.NO)
                                AND wd.begin_date = (
                                                     select
                                                       MAX(wd1.begin_date)
                                                     from
                                                       well_descriptions wd1
                                                     where
                                                       wd1.geology_no = NVL(wg.parent_geology_no, wg.NO)
                                                       AND wd1.begin_date > = wg.date_begin
                                                       AND wd1.end_date < = wg.date_end
                                                       AND wd1.begin_date < ADD_MONTHS(:dt_pdate, 1)
                                                    )
                                AND org_m.org_no = wd.organization_no
                             ) LIKE ' %ЦДНГ% '
                          THEN - 999
                        ELSE
                          NVL((
                               select
                                 v0.object_no_value
                               from
                                 tfc$point_values v0
                                 , tfc$points p0
                               where
                                 p0.structure_no = :dt_pstru_skw
                                 AND p0.objects_list = TO_CHAR(a1_t.w_no) || '
                                 , ' || TO_CHAR(:dt_p_r_sost) || '
                                 , ' || TO_CHAR(a1_t.wg_object_no)
                                 AND v0.point_no = p0.NO
                                 AND v0.value_date > TRUNC(:dt_pdate, ' YYYY ')
                                 AND v0.value_date < = ADD_MONTHS(:dt_pdate, 1)
                                 AND v0.value_date = (
                                                      select
                                                        MAX(tpv.value_date)
                                                      from
                                                        tfc$point_values tpv
                                                        , DICTIONARY dic
                                                      where
                                                        tpv.point_no = v0.point_no
                                                        AND tpv.value_date < = ADD_MONTHS(:dt_pdate, 1)
                                                        AND tpv.value_date > TRUNC(:dt_pdate, ' YYYY ')
                                                        AND dic.NO = tpv.object_no_value
                                                        AND dic.code < = 29
                                                        AND dic.code > = 21
                                                     )
                                 AND ROWNUM < 2
                              ), (
                                  select
                                    v0.object_no_value
                                  from
                                    tatoil$well_geologyes wg3
                                    , tfc$point_values v0
                                    , tfc$points p0
                                    , DICTIONARY dic
                                  where
                                    wg3.well_no = a1_t.w_no
                                    AND wg3.type_object = ' NEW '
                                    AND wg3.date_begin < ADD_MONTHS(:dt_pdate, 1)
                                    AND wg3.date_end > TRUNC(:dt_pdate, ' YYYY ')
                                    AND wg3.is_main_fond = ' T '
                                    AND p0.structure_no = :dt_pstru_skw
                                    AND p0.objects_list = TO_CHAR(wg3.well_no) || '
                                    , ' || TO_CHAR(:dt_p_r_sost) || '
                                    , ' || TO_CHAR(wg3.object_no)
                                    AND v0.point_no = p0.NO
                                    AND v0.value_date > TRUNC(:dt_pdate, ' YYYY ')
                                    AND v0.value_date < = ADD_MONTHS(:dt_pdate, 1)
                                    AND v0.value_date = (
                                                         select
                                                           MAX(tpv.value_date)
                                                         from
                                                           tfc$point_values tpv
                                                           , well_descriptions wd3
                                                           , (
                                                              select
                                                                oe.NO org_no
                                                              from
                                                                organizations oe
                                                              where
                                                                oe.type_no = :dt_poe3
                                                                OR oe.type_no = :dt_poe19
                                                             ) org3
                                                           , DICTIONARY dic
                                                         where
                                                           wd3.geology_no = NVL(wg3.parent_geology_no, wg3.NO)
                                                           AND wd3.organization_no = org3.org_no
                                                           AND wd3.begin_date > = wg3.date_begin
                                                           AND wd3.end_date < = wg3.date_end
                                                           AND wd3.begin_date < ADD_MONTHS(:dt_pdate, 1)
                                                           AND wd3.end_date > TRUNC(:dt_pdate, ' YYYY ')
                                                           AND tpv.point_no = v0.point_no
                                                           AND tpv.value_date < = wd3.end_date
                                                           AND tpv.value_date > wd3.begin_date
                                                           AND tpv.value_date < = ADD_MONTHS(:dt_pdate, 1)
                                                           AND tpv.value_date > TRUNC(:dt_pdate, ' YYYY ')
                                                           AND dic.NO = tpv.object_no_value
                                                           AND dic.code < = 29
                                                           AND dic.code > = 21
                                                        )
                                    AND dic.NO = v0.object_no_value
                                    AND dic.code < = 29
                                    AND dic.code > = 21
                                    AND ROWNUM < 2
                                 ))
                      END) sost_t
                 from
                   (
                    select
                      DISTINCT a.report_name
                      , a.w_no
                      , a.wg_object_no
                      , a.wd_begin_date
                      , a.wd_end_date
                      , v.value_date
                      , a.ds_no
                    from
                      (
                       select
                         w.report_name
                         , wg.well_no w_no
                         , wg.object_no wg_object_no
                         , wd.begin_date wd_begin_date
                         , wd.end_date wd_end_date
                         , org.ds_no
                       from
                         wells w
                         , tatoil$well_geologyes wg
                         , well_descriptions wd
                         , (
                            select
                              oe.NO org_no
                              , oe.domain_shop_no ds_no
                            from
                              organizations oe
                            where
                              (oe.domain_org_no = :dt_porg
                               or oe.DOMAIN_COMPANY_NO = :dt_porg)
                              AND (oe.type_no = :dt_poe3
                                   OR oe.type_no = :dt_poe19)
                           ) org
                       where
                         wg.well_no = w.NO
                         AND wg.type_object = ' NEW '
                         AND wg.date_begin < ADD_MONTHS(:dt_pdate, 1)
                         AND wg.date_end > TRUNC(:dt_pdate, ' YYYY ')
                         AND wd.geology_no = NVL(wg.parent_geology_no, wg.NO)
                         AND wd.organization_no = org.org_no
                         AND wd.begin_date < wg.date_end
                         AND wd.end_date > wg.date_begin
                         AND wd.begin_date < ADD_MONTHS(:dt_pdate, 1)
                         AND wd.end_date > TRUNC(:dt_pdate, ' YYYY ')
                         AND wd.state_no < > :dt_pstate1
                         AND wd.state_no < > :dt_pstate2
                         AND (EXISTS (
                                      select
                                        1
                                      from
                                        tfc$point_values v0
                                        , tfc$points p0
                                        , DICTIONARY dic
                                      where
                                        p0.structure_no = :dt_pstru_skw
                                        AND p0.objects_list = TO_CHAR(wg.well_no) || '
                                        , ' || TO_CHAR(:dt_p_r_sost) || '
                                        , ' || TO_CHAR(wg.object_no)
                                        AND v0.point_no = p0.NO
                                        AND v0.value_date > TRUNC(:dt_pdate, ' YYYY ')
                                        AND v0.value_date < = ADD_MONTHS(:dt_pdate, 1)
                                        AND v0.value_date > wd.begin_date
                                        AND v0.value_date < = wd.end_date
                                        AND dic.NO = v0.object_no_value
                                        AND dic.code < = 29
                                        AND dic.code > = 21
                                        AND ROWNUM < 2
                                     )
                              OR EXISTS (
                                         select
                                           1
                                         from
                                           tatoil$well_geologyes wg3
                                           , well_descriptions wd3
                                           , (
                                              select
                                                oe.NO org_no
                                                , oe.domain_shop_no ds_no
                                              from
                                                organizations oe
                                              where
                                                oe.type_no = :dt_poe3
                                                OR oe.type_no = :dt_poe19
                                             ) org3
                                           , tfc$point_values v0
                                           , tfc$points p0
                                           , DICTIONARY dic
                                         where
                                           wg3.well_no = wg.well_no
                                           AND wg3.type_object = ' NEW '
                                           AND wg3.date_begin < ADD_MONTHS(:dt_pdate, 1)
                                           AND wg3.date_end > TRUNC(:dt_pdate, ' YYYY ')
                                           AND wg3.date_begin < wg.date_end
                                           AND wg3.date_end > wg.date_begin
                                           AND wg3.is_main_fond = ' T '
                                           AND wd3.geology_no = NVL(wg3.parent_geology_no, wg3.NO)
                                           AND wd3.organization_no = org3.org_no
                                           AND wd3.begin_date < wg3.date_end
                                           AND wd3.end_date > wg3.date_begin
                                           AND wd3.begin_date < ADD_MONTHS(:dt_pdate, 1)
                                           AND wd3.end_date > TRUNC(:dt_pdate, ' YYYY ')
                                           AND p0.structure_no = :dt_pstru_skw
                                           AND p0.objects_list = TO_CHAR(wg3.well_no) || '
                                           , ' || TO_CHAR(:dt_p_r_sost) || '
                                           , ' || TO_CHAR(wg3.object_no)
                                           AND v0.point_no = p0.NO
                                           AND v0.value_date > TRUNC(:dt_pdate, ' YYYY ')
                                           AND v0.value_date < = ADD_MONTHS(:dt_pdate, 1)
                                           AND v0.value_date > wd3.begin_date
                                           AND v0.value_date < = wd3.end_date
                                           AND dic.NO = v0.object_no_value
                                           AND dic.code < = 29
                                           AND dic.code > = 21
                                           AND ROWNUM < 2
                                        ))
                      ) a
                      , tfc$point_values v
                      , tfc$points p
                      , (
                         select
                           NO no1
                         from
                           DICTIONARY da
                         where
                           da.NO = :dt_p_ag34
                           OR da.NO = :dt_p_ag35
                           OR da.NO = :dt_p_ag45
                           OR da.NO = :dt_p_ag46
                           OR da.NO = :dt_p_ag47
                           OR da.NO = :dt_p_ag22
                           OR da.NO = :dt_p_ag48
                           OR da.NO = :dt_p_ag84
                           OR da.NO = :dt_p_ag69
                           OR da.NO = :dt_p_ag1
                           OR da.NO = :dt_p_ag0
                        ) ag_no
                      , (
                         select
                           NO
                         from
                           tfc$characteristics c2
                         where
                           c2.upper_code = ' 592 '
                           OR c2.upper_code = ' 593 '
                        ) tc
                    where
                      p.structure_no = :dt_pstru_mera
                      AND p.objects_list = TO_CHAR(a.w_no) || '
                      , ' || TO_CHAR(tc.NO) || '
                      , ' || TO_CHAR(a.wg_object_no) || '
                      , ' || TO_CHAR(ag_no.no1)
                      AND v.point_no = p.NO
                      AND v.value_date > = a.wd_begin_date
                      AND v.value_date < a.wd_end_date
                      AND v.value_date > = TRUNC(:dt_pdate, ' YYYY ')
                      AND v.value_date < ADD_MONTHS(:dt_pdate, 1)
                      AND v.VALUE > 0
                      AND COALESCE((
                                    select
                                      1
                                    from
                                      tfc$point_values v0
                                      , tfc$points p0
                                      , DICTIONARY dic
                                    where
                                      p0.structure_no = :dt_pstru_skw
                                      AND p0.objects_list = TO_CHAR(a.w_no) || '
                                      , ' || TO_CHAR(:dt_p_r_sost) || '
                                      , ' || TO_CHAR(a.wg_object_no)
                                      AND v0.point_no = p0.NO
                                      AND v0.value_date > TRUNC(:dt_pdate, ' YYYY ')
                                      AND v0.value_date < = ADD_MONTHS(:dt_pdate, 1)
                                      AND v0.value_date > a.wd_begin_date
                                      AND v0.value_date < = a.wd_end_date
                                      AND v0.value_date = (
                                                           select
                                                             MIN(tpv.value_date)
                                                           from
                                                             tfc$point_values tpv
                                                           where
                                                             tpv.point_no = v0.point_no
                                                             AND tpv.value_date < = a.wd_end_date
                                                             AND tpv.value_date > a.wd_begin_date
                                                             AND tpv.value_date > v.value_date
                                                          )
                                      AND dic.NO = v0.object_no_value
                                      AND dic.code < = 29
                                      AND dic.code > = 21
                                      AND ROWNUM < 2
                                   ), (
                                       select
                                         1
                                       from
                                         tatoil$well_geologyes wg3
                                         , well_descriptions wd3
                                         , (
                                            select
                                              oe.NO org_no
                                            from
                                              organizations oe
                                            where
                                              oe.type_no = :dt_poe3
                                              OR oe.type_no = :dt_poe19
                                           ) org3
                                         , tfc$point_values v0
                                         , tfc$points p0
                                         , DICTIONARY dic
                                       where
                                         wg3.well_no = a.w_no
                                         AND wg3.type_object = ' NEW '
                                         AND wg3.date_begin < ADD_MONTHS(:dt_pdate, 1)
                                         AND wg3.date_end > TRUNC(:dt_pdate, ' YYYY ')
                                         AND wg3.is_main_fond = ' T '
                                         AND wd3.geology_no = NVL(wg3.parent_geology_no, wg3.NO)
                                         AND wd3.organization_no = org3.org_no
                                         AND wd3.begin_date < wg3.date_end
                                         AND wd3.end_date > wg3.date_begin
                                         AND wd3.begin_date < ADD_MONTHS(:dt_pdate, 1)
                                         AND wd3.end_date > TRUNC(:dt_pdate, ' YYYY ')
                                         AND p0.structure_no = :dt_pstru_skw
                                         AND p0.objects_list = TO_CHAR(wg3.well_no) || '
                                         , ' || TO_CHAR(:dt_p_r_sost) || '
                                         , ' || TO_CHAR(wg3.object_no)
                                         AND v0.point_no = p0.NO
                                         AND v0.value_date > TRUNC(:dt_pdate, ' YYYY ')
                                         AND v0.value_date < = ADD_MONTHS(:dt_pdate, 1)
                                         AND v0.value_date > wd3.begin_date
                                         AND v0.value_date < = wd3.end_date
                                         AND v0.value_date = (
                                                              select
                                                                MIN(tpv.value_date)
                                                              from
                                                                tfc$point_values tpv
                                                              where
                                                                tpv.point_no = v0.point_no
                                                                AND tpv.value_date < = wd3.end_date
                                                                AND tpv.value_date > wd3.begin_date
                                                                AND tpv.value_date > v.value_date
                                                                AND tpv.value_date < = ADD_MONTHS(v.value_date, 1)
                                                             )
                                         AND dic.NO = v0.object_no_value
                                         AND dic.code < = 29
                                         AND dic.code > = 21
                                         AND ROWNUM < 2
                                      ), 0) > 0
                   ) a1_t
                   , tatoil$geo_work_objects obr
                   , tatoil$ext_elements ee
                 where
                   obr.NO = a1_t.wg_object_no
                   AND ee.NO = obr.ext_element_no
                ) a2_t
              GROUP by
                a2_t.w_no
                , a2_t.wg_object_no
                , a2_t.domain_plo_no
                , a2_t.domain_mest_no
                , a2_t.sost_t
                , a2_t.v_mes
             ) a3_t
           GROUP by
             a3_t.w_no
             , a3_t.domain_plo_no
             , a3_t.domain_mest_no
             , a3_t.sost_t
             , a3_t.v_mes
          ) t3
      where
        a3.w_no = t3.w_no(+)
        AND a3.v_mes = t3.v_mes(+)
        AND a3.domain_mest_no = t3.domain_mest_no(+)
        AND a3.sost = t3.sost_t(+)
     ) a4
   GROUP by
     a4.report_name
     , a4.w_no
     , a4.dorg_no
     , a4.domain_plo_no
     , a4.domain_mest_no
     , a4.sost
  ) a5
